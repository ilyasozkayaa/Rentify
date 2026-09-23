using System.Text.Json;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyApplication.Query.SearchRentals;
using RentifyApplication.Query.GetPendingRentalListings;
using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Entities;
using RentifyDomain.Enum;

namespace RentifyUnitTests.Query;

public sealed class SearchRentalsQueryHandlerTests
{
    [Fact]
    public async Task Should_throw_business_exception_when_dates_are_missing()
    {
        var intent = CreateIntent(startDate: null, endDate: null);
        var handler = CreateHandler(intent, []);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(new SearchRentalsQuery("Antalya villa search"), CancellationToken.None));

        Assert.Equal(BusinessErrorCode.SearchCriteriaRequired, exception.Code);
    }

    [Fact]
    public async Task Should_filter_vehicle_attributes_and_map_the_result()
    {
        var intent = CreateIntent(
            rentalType: RentalType.Vehicle,
            startDate: new DateOnly(2026, 6, 1),
            endDate: new DateOnly(2026, 6, 7),
            vehicleCriteria: new VehicleSearchCriteria(
                Brand: "Toyota",
                Model: "Corolla",
                ModelYear: 2024,
                Transmission: TransmissionType.Automatic,
                FuelType: FuelType.Hybrid,
                Seats: 4));

        var matchingProduct = new RentableProduct
        {
            Id = 1,
            RentalType = (int)RentalType.Vehicle,
            CityCode = (int)CityCode.Antalya,
            Title = "Toyota Corolla",
            Price = 1500,
            Currency = "TRY",
            Attributes = JsonDocument.Parse("""
                {
                  "Brand": "Toyota",
                  "Model": "Corolla",
                  "ModelYear": 2024,
                  "Transmission": "Automatic",
                  "FuelType": "Hybrid",
                  "Seats": 5
                }
                """)
        };

        var nonMatchingProduct = new RentableProduct
        {
            Id = 2,
            RentalType = (int)RentalType.Vehicle,
            CityCode = (int)CityCode.Antalya,
            Title = "Renault Clio",
            Price = 1000,
            Currency = "TRY",
            Attributes = JsonDocument.Parse("""
                {
                  "Brand": "Renault",
                  "Model": "Clio",
                  "ModelYear": 2024,
                  "Transmission": "Manual",
                  "FuelType": "Gasoline",
                  "Seats": 5
                }
                """)
        };

        var handler = CreateHandler(intent, [matchingProduct, nonMatchingProduct]);

        var response = await handler.Handle(new SearchRentalsQuery("Antalya hybrid Toyota Corolla"), CancellationToken.None);

        var result = Assert.Single(response.Results);
        Assert.Equal(1, result.Id);
        Assert.Equal("Toyota Corolla", result.Title);
        Assert.False(response.HasNextPage);
    }

    [Fact]
    public async Task Should_return_has_next_page_when_more_products_are_available()
    {
        var intent = CreateIntent(startDate: new DateOnly(2026, 6, 1), endDate: new DateOnly(2026, 6, 7));
        var products = Enumerable.Range(1, 1001)
            .Select(id => new RentableProduct
            {
                Id = id,
                RentalType = (int)RentalType.Property,
                CityCode = (int)CityCode.Antalya,
                Title = $"Villa {id}",
                Price = 1000,
                Currency = "TRY"
            })
            .ToList();

        var handler = CreateHandler(intent, products);

        var response = await handler.Handle(new SearchRentalsQuery("Antalya villa search"), CancellationToken.None);

        Assert.Equal(1000, response.Results.Count);
        Assert.True(response.HasNextPage);
    }

    [Fact]
    public async Task Should_return_only_pending_listings_for_admin_review()
    {
        var pending = new RentableProduct
        {
            Id = 3,
            OwnerUserId = 12,
            RentalType = (int)RentalType.Villa,
            CityCode = (int)CityCode.Antalya,
            Title = "Pending villa",
            Price = 3000,
            Status = (int)RentableProductStatus.Pending
        };
        var repository = new FakeRentableProductRepository(
        [
            pending,
            new RentableProduct { Id = 4, Title = "Active listing", Status = (int)RentableProductStatus.Active }
        ]);
        var handler = new GetPendingRentalListingsQueryHandler(repository);

        var response = await handler.Handle(new GetPendingRentalListingsQuery(), CancellationToken.None);

        var result = Assert.Single(response.Results);
        Assert.Equal(pending.Id, result.Id);
        Assert.Equal(pending.OwnerUserId, result.OwnerUserId);
        Assert.Equal(nameof(RentalType.Villa), result.RentalType);
        Assert.False(response.HasNextPage);
    }

    [Fact]
    public async Task Should_return_a_page_of_pending_listings_and_indicate_more_results()
    {
        var listings = Enumerable.Range(1, 3)
            .Select(id => new RentableProduct
            {
                Id = id,
                Title = $"Pending {id}",
                Status = (int)RentableProductStatus.Pending
            })
            .ToList();
        var handler = new GetPendingRentalListingsQueryHandler(new FakeRentableProductRepository(listings));

        var response = await handler.Handle(new GetPendingRentalListingsQuery(Page: 1, PageSize: 2), CancellationToken.None);

        Assert.Equal(2, response.Results.Count);
        Assert.True(response.HasNextPage);
        Assert.Equal([1, 2], response.Results.Select(x => x.Id));
    }

    private static SearchRentalsQueryHandler CreateHandler(SearchIntent intent, List<RentableProduct> products)
    {
        return new SearchRentalsQueryHandler(new FakeSearchIntentService(intent), new FakeRentableProductRepository(products));
    }

    private static SearchIntent CreateIntent(RentalType rentalType = RentalType.Property, DateOnly? startDate = null, DateOnly? endDate = null, VehicleSearchCriteria? vehicleCriteria = null)
    {
        return new SearchIntent(rentalType, (int)CityCode.Antalya, startDate, endDate, null, null, null, "TRY", vehicleCriteria, null, null, 1);
    }

    private sealed class FakeSearchIntentService(SearchIntent intent) : ISearchIntentService
    {
        public Task<SearchIntent> CreateIntentAsync(SearchRentalsQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(intent);
        }
    }

    private sealed class FakeRentableProductRepository(List<RentableProduct> products) : IRentableProductRepository
    {
        public Task<List<RentableProduct>> SearchAsync(SearchIntent searchIntent, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(products.ToList());
        }

        public Task<List<RentableProduct>> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(products.Where(x => x.Status == (int)RentableProductStatus.Pending)
                .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
                .Skip((page - 1) * pageSize).Take(pageSize + 1).ToList());
        }

        public Task<RentableProduct?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(products.SingleOrDefault(x => x.Id == id));

        public Task<List<RentableProduct>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(products.ToList());

        public Task AddAsync(RentableProduct entity, CancellationToken cancellationToken = default)
        {
            products.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(RentableProduct entity)
        {
        }

        public void Remove(RentableProduct entity)
        {
            products.Remove(entity);
        }
    }
}
