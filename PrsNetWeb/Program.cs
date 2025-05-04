using Microsoft.EntityFrameworkCore;
using PrsNetWeb.Models;

namespace PrsNetWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers().AddJsonOptions(opt =>
			{
				opt.JsonSerializerOptions.ReferenceHandler =
				System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
			});
			builder.Services.AddDbContext<PRSDBContext>(
				options => options.UseSqlServer(builder.Configuration.GetConnectionString("PRSDBConnectionString"))
				);
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowAngularApp",
					builder => builder.WithOrigins("http://localhost:4200")
									  .AllowAnyHeader()
									  .AllowAnyMethod());
			});


			var app = builder.Build();

			// Configure the HTTP request pipeline.
			app.UseCors("AllowAngularApp");
			app.UseStaticFiles();
			app.UseHttpsRedirection();
			
			app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
