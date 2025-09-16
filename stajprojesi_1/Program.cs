using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using stajprojesi_1.Services;

namespace stajprojesi_1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Serilog'u en başta konfigüre et
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("=== UYGULAMA BAŞLIYOR ===");
                Console.WriteLine("Logger başlatıldı!");
                
                var builder = WebApplication.CreateBuilder(args);

                // Serilog'u host'a bağla - appsettings.json'dan oku
                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithThreadId()
                    .Enrich.WithMachineName());
                
                // Add services to the container.
                builder.Services.AddControllersWithViews();
                builder.Services.AddDistributedMemoryCache();
                builder.Services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

                // Logging service'i ekle
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddScoped<ILoggingService, LoggingService>();

                var app = builder.Build();

                Log.Information("🏗️ Uygulama build edildi ve hazır");

                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseRouting();
                app.UseSession();

                // Gelişmiş logging middleware
                app.Use(async (context, next) =>
                {
                    var user = context.Session?.GetString("KullaniciAdi") ?? "Anonim";
                    var path = context.Request?.Path.Value ?? "Unknown";
                    var method = context.Request?.Method ?? "Unknown";
                    var userAgent = context.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
                    var ip = context.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
                    
                    // Giriş logu
                    Log.Information("🌐 YENİ İSTEK BAŞLADI");
                    Log.Information("👤 Kullanıcı: {User}", user);
                    Log.Information("📍 Yol: {Method} {Path}", method, path);
                    Log.Information("🌍 IP Adresi: {IP}", ip);
                    Log.Information("🔍 User Agent: {UserAgent}", userAgent);
                    
                    var startTime = DateTime.UtcNow;

                    await next();

                    var endTime = DateTime.UtcNow;
                    var duration = endTime - startTime;
                    
                    // Çıkış logu
                    Log.Information("✅ İSTEK TAMAMLANDI");
                    Log.Information("⏱️ Toplam süre: {Duration}ms", duration.TotalMilliseconds);
                    Log.Information("📊 HTTP Durum: {StatusCode}", context.Response?.StatusCode);
                    Log.Information("📏 Yanıt boyutu: {ContentLength} bytes", context.Response?.ContentLength ?? 0);
                    Log.Information("---");
                });

                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");

                Log.Information("🚀 Uygulama başlatılıyor...");
                Log.Information("🌐 Port: 5287");
                Log.Information("🔗 URL: http://localhost:5287");
                Console.WriteLine("Uygulama başlatılıyor...");
                
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "💥 Uygulama başlatılamadı - Kritik hata!");
                Log.Fatal("❌ Hata detayı: {ErrorMessage}", ex.Message);
                Log.Fatal("🔍 Hata türü: {ExceptionType}", ex.GetType().Name);
                Console.WriteLine($"HATA: {ex.Message}");
            }
            finally
            {
                Log.Information("🔄 Uygulama kapatılıyor...");
                Log.CloseAndFlush();
                Console.WriteLine("Logger kapatıldı");
            }
        }
    }
}
