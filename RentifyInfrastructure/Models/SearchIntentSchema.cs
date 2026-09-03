namespace RentifyInfrastructure.Models;

public static class SearchIntentSchema
{
    public static BinaryData Create()
    {
        var schema = new
        {
            type = "object",
            additionalProperties = false,

            properties = new
            {
                rentalType = new
                {
                    type = "string",
                    @enum = new[]
                    {
                        "Vehicle",
                        "Property",
                        "Hotel",
                        "Villa",
                        "Unknown"
                    }
                },

                cityCode = new
                {
                    type = "integer"
                },

                currency = new
                {
                    type = new[] { "string", "null" },
                    @enum = new[]
                    {
                        "TRY",
                        "USD",
                        "EUR",
                        "GBP",
                        null
                    }
                },

                startDate = new
                {
                    type = new[] { "string", "null" }
                },

                endDate = new
                {
                    type = new[] { "string", "null" }
                },

                searchText = new
                {
                    type = new[] { "string", "null" }
                },

                minPrice = new
                {
                    type = new[] { "number", "null" }
                },

                maxPrice = new
                {
                    type = new[] { "number", "null" }
                },

                vehicleCriteria = new
                {
                    type = new[] { "object", "null" },

                    properties = new
                    {
                        brand = new
                        {
                            type = new[] { "string", "null" }
                        },

                        model = new
                        {
                            type = new[] { "string", "null" }
                        },

                        modelYear = new
                        {
                            type = new[] { "integer", "null" }
                        },

                        transmission = new
                        {
                            type = new[] { "string", "null" },
                            @enum = new[]
                            {
                                "Manual",
                                "Automatic",
                                null
                            }
                        },

                        fuelType = new
                        {
                            type = new[] { "string", "null" },
                            @enum = new[]
                            {
                                "Gasoline",
                                "Diesel",
                                "Electric",
                                "Hybrid",
                                "LPG",
                                null
                            }
                        },

                        seats = new
                        {
                            type = new[] { "integer", "null" }
                        }
                    },

                    required = new[]
                    {
                        "brand",
                        "model",
                        "modelYear",
                        "transmission",
                        "fuelType",
                        "seats"
                    }
                },

                propertyCriteria = new
                {
                    type = new[] { "object", "null" },
                    properties = new
                    {
                        bedrooms = new
                        {
                            type = new[] { "integer", "null" }
                        },
                        bathrooms = new
                        {
                            type = new[] { "integer", "null" }
                        },
                        seaView = new
                        {
                            type = new[] { "boolean", "null" }
                        },
                        detached = new
                        {
                            type = new[] { "boolean", "null" }
                        },
                        furnished = new
                        {
                            type = new[] { "boolean", "null" }
                        },
                        pool = new
                        {
                            type = new[] { "boolean", "null" }
                        },
                        garden = new
                        {
                            type = new[] { "boolean", "null" }
                        }
                    },
                    required = new[]
                    {
                        "bedrooms",
                        "bathrooms",
                        "seaView",
                        "detached",
                        "furnished",
                        "pool",
                        "garden"
                    }
                },

                hotelCriteria = new
                {
                    type = new[] { "object", "null" },

                    properties = new
                    {
                        stars = new
                        {
                            type = new[] { "integer", "null" }
                        },

                        breakfastIncluded = new
                        {
                            type = new[] { "boolean", "null" }
                        },

                        pool = new
                        {
                            type = new[] { "boolean", "null" }
                        },

                        openBuffet = new
                        {
                            type = new[] { "boolean", "null" }
                        },

                        allInclusive = new
                        {
                            type = new[] { "boolean", "null" }
                        },

                        restaurant = new
                        {
                            type = new[] { "boolean", "null" }
                        },

                        gym = new
                        {
                            type = new[] { "boolean", "null" }
                        },

                        spa = new
                        {
                            type = new[] { "boolean", "null" }
                        },

                        guestCapacity = new
                        {
                            type = new[] { "integer", "null" }
                        },

                        parking = new
                        {
                            type = new[] { "boolean", "null" }
                        },

                        seaView = new
                        {
                            type = new[] { "boolean", "null" }
                        },

                        airConditioning = new
                        {
                            type = new[] { "boolean", "null" }
                        },

                        wifi = new
                        {
                            type = new[] { "boolean", "null" }
                        }
                    },

                    required = new[]
                    {
                        "stars",
                        "breakfastIncluded",
                        "pool",
                        "openBuffet",
                        "allInclusive",
                        "restaurant",
                        "gym",
                        "spa",
                        "guestCapacity",
                        "parking",
                        "seaView",
                        "airConditioning",
                        "wifi"
                    }
                }
            },

            required = new[]
            {
                "rentalType",
                "cityCode",
                "startDate",
                "endDate",
                "searchText",
                "minPrice",
                "maxPrice",
                "currency",
                "vehicleCriteria",
                "propertyCriteria",
                "hotelCriteria"
            }
        };

        return BinaryData.FromObjectAsJson(schema);
    }
}
