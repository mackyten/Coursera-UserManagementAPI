using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults; // <--- THIS IS CRUCIAL
using Microsoft.AspNetCore.Routing;         // Also important for IEndpointRouteBuilder
using UserManagementAPI;                    // Assuming UserModel is in this namespace

namespace UserManagementAPI // Ensure this namespace is consistent
{
    public static class UserEndpoints
    {
        private static readonly List<UserModel> Users = new()
        {
            new UserModel { Id = 1, Username = "alice", Email = "alice@example.com" },
            new UserModel { Id = 2, Username = "bob", Email = "bob@example.com" }
        };
        private static int _nextId = 3;

        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var userGroup = app.MapGroup("/api/users").WithTags("Users");

            userGroup.MapGet("/", () => TypedResults.Ok(Users));

            // THIS IS THE ENDPOINT IN QUESTION
            userGroup.MapGet("/{id}", async (int id) =>
            {
                var user = Users.FirstOrDefault(u => u.Id == id);
                return user is not null ? TypedResults.Ok(user) : TypedResults.NotFound();
            })
            .WithName("GetUserById");

            userGroup.MapPost("/", (UserModel user) =>
            {
                user.Id = _nextId++;
                Users.Add(user);
                return TypedResults.CreatedAtRoute(user, "GetUserById", new { id = user.Id });
            });

            userGroup.MapPut("/{id}", (int id, UserModel inputUser) =>
            {
                var user = Users.FirstOrDefault(u => u.Id == id);
                if (user is null)
                {
                    return TypedResults.NotFound();
                }
                user.Username = inputUser.Username;
                user.Email = inputUser.Email;
                return TypedResults.NoContent();
            });

            userGroup.MapDelete("/{id}", (int id) =>
            {
                var user = Users.FirstOrDefault(u => u.Id == id);
                if (user is null) return TypedResults.NotFound();
                Users.Remove(user);
                return TypedResults.NoContent();
            });
        }
    }
}