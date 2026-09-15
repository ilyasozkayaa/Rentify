using RentifyApplication.Command.Login;
using RentifyApplication.Command.Register;
using RentifyApplication.Exceptions;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyDomain.Entities;

namespace RentifyUnitTests.Command;

public sealed class AuthenticationHandlerTests
{
    [Fact]
    public async Task Register_should_normalize_email_and_store_a_hash()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RegisterCommandHandler(repository, new FakePasswordHasherService(), unitOfWork);

        await handler.Handle(new RegisterCommand("  USER@EXAMPLE.COM ", "password123", " Ada ", " Lovelace "), CancellationToken.None);

        var user = Assert.Single(repository.Users);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("hashed:password123", user.PasswordHash);
        Assert.Equal("Ada", user.FirstName);
        Assert.Equal("Lovelace", user.LastName);
    }

    [Fact]
    public async Task Register_should_reject_an_existing_email()
    {
        var repository = new FakeUserRepository
        {
            Users =
            [
                new User { Id = 1, Email = "user@example.com" }
            ]
        };
        var handler = new RegisterCommandHandler(repository, new FakePasswordHasherService(), new FakeUnitOfWork());

        var exception = await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(new RegisterCommand("USER@example.com", "password123", "Ada", "Lovelace"), CancellationToken.None));

        Assert.Equal("EmailAlreadyExists", exception.Code.ToString());
    }

    [Fact]
    public async Task Login_should_return_a_token_for_valid_credentials()
    {
        var user = new User
        {
            Id = 7,
            Email = "user@example.com",
            FirstName = "Ada",
            LastName = "Lovelace",
            PasswordHash = "hashed:password123",
            IsActive = true
        };
        var handler = new LoginCommandHandler(new FakeUserRepository { Users = [user] }, new FakePasswordHasherService(), new FakeJwtTokenService());

        var response = await handler.Handle(new LoginCommand("USER@example.com", "password123"), CancellationToken.None);

        Assert.Equal(user.Id, response.Id);
        Assert.Equal("token-7", response.AccessToken);
    }

    [Fact]
    public async Task Login_should_reject_an_inactive_user()
    {
        var user = new User
        {
            Id = 7,
            Email = "user@example.com",
            PasswordHash = "hashed:password123",
            IsActive = false
        };
        var handler = new LoginCommandHandler(new FakeUserRepository { Users = [user] }, new FakePasswordHasherService(), new FakeJwtTokenService());

        var exception = await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(new LoginCommand("user@example.com", "password123"), CancellationToken.None));

        Assert.Equal("AccountInactive", exception.Code.ToString());
    }

    private sealed class FakePasswordHasherService : IPasswordHasherService
    {
        public string Hash(string password) => $"hashed:{password}";

        public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public string GenerateToken(int userId, string email, bool isAdmin) => $"token-{userId}";
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Users { get; set; } = [];

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(Users.SingleOrDefault(x => x.Email == email));

        public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(Users.SingleOrDefault(x => x.Id == id));

        public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(Users.ToList());

        public Task AddAsync(User entity, CancellationToken cancellationToken = default)
        {
            entity.Id = Users.Count + 1;
            Users.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(User entity)
        {
        }

        public void Remove(User entity)
        {
            Users.Remove(entity);
        }
    }
}
