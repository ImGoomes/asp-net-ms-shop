﻿namespace Identity.API.Endpoints
{
    public partial class AuthorizationEndpoint
    {
        public static async Task<IResult> RegisterUser(
            RegisterModel registerData,
            UserManager<IdentityUser> userManager)
        {
            if (registerData.TryValidate(out var validationErrors) == false)
            {
                return Results.BadRequest(validationErrors);
            }

            var user = await userManager.FindByEmailAsync(registerData.Email);
            if (user != null)
            {
                return Results.Conflict();
            }

            user = new IdentityUser { UserName = registerData.Username, Email = registerData.Email };
            var result = await userManager.CreateAsync(user, registerData.Password);
            if (result.Succeeded == false)
            {
                return Results.BadRequest(result.Errors.Select(e => e.Description));
            }

            result = await userManager.AddToRoleAsync(user, "User");
            if (result.Succeeded == false)
            {
                return Results.BadRequest(result.Errors.Select(e => e.Description));
            }

            return Results.Ok();
        }
    }
}
