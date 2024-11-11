using System.Security.Claims;

namespace Presentation.Configuration
{
    public class UserMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserMiddleWare> _logger;

        public UserMiddleWare(RequestDelegate next, ILogger<UserMiddleWare> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (context.User.Identity.IsAuthenticated)
            {

                var email = context.User.Claims.FirstOrDefault(c => c.Type == "emails" || c.Type == "email" || c.Type == "preferred_username")?.Value;
                var userId = context.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var firstName = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;

                var lastName = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
                var name = $"{firstName} {lastName}";//B2C
                var fullName = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName || c.Type == "name")?.Value;//B2B
                string fName = string.Empty;
                string lName = string.Empty;

                if (!string.IsNullOrEmpty(fullName))
                {
                    var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    fName = nameParts[0];
                    if (nameParts.Length > 1)
                    {
                        lName = string.Join(" ", nameParts.Skip(1));
                    }
                }

                context.Items["FName"] = fName;
                context.Items["LName"] = lName;
                context.Items["FulName"] = name;

                context.Items["LastName"] = lastName;
                context.Items["FirstName"] = firstName;
                context.Items["Email"] = email;

                context.Items["UserId"] = userId;
            }
            else
            {
                _logger.LogWarning("User is not authenticated.");
            }


            await _next(context);
        }
    }
}
