using ConnectorTestTask.Presentation.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ConnectorTestTask.Presentation.Extensions
{
    public static class WebApplicationExtension
    {
        public static void ConfigureApplication(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Portfolio}/{action=Index}/{id?}");
        }
    }
}