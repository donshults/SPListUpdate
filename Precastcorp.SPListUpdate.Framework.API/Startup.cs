using Owin;

namespace Precastcorp.SPListUpdate.Framework.API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
