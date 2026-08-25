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

                location = new
                {
                    type = new[] { "string", "null" }
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

                    additionalProperties = false,

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
                            type = new[] { "string", "null" }
                        },

                        fuelType = new
                        {
                            type = new[] { "string", "null" }
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

                    additionalProperties = false,

                    properties = new
                    {
                        propertyType = new
                        {
                            type = new[] { "string", "null" }
                        },

                        seaView = new
                        {
                            type = new[] { "boolean", "null" }
                        },

                        detached = new
                        {
                            type = new[] { "boolean", "null" }
                        },

                        bedrooms = new
                        {
                            type = new[] { "integer", "null" }
                        },

                        pool = new
                        {
                            type = new[] { "boolean", "null" }
                        }
                    },

                    required = new[]
                    {
                        "propertyType",
                        "seaView",
                        "detached",
                        "bedrooms",
                        "pool"
                    }
                },

                hotelCriteria = new
                {
                    type = new[] { "object", "null" },

                    additionalProperties = false,

                    properties = new
                    {
                        minimumStars = new
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

                        roomType = new
                        {
                            type = new[] { "string", "null" }
                        }
                    },

                    required = new[]
                    {
                        "minimumStars",
                        "breakfastIncluded",
                        "pool",
                        "roomType"
                    }
                }
            },

            required = new[]
            {
                "rentalType",
                "location",
                "startDate",
                "endDate",
                "searchText",
                "minPrice",
                "maxPrice",
                "vehicleCriteria",
                "propertyCriteria",
                "hotelCriteria"
            }
        };

        return BinaryData.FromObjectAsJson(schema);
    }
}
