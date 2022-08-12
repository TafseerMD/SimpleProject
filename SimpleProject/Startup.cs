using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SimpleProject.Startup))]
namespace SimpleProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
