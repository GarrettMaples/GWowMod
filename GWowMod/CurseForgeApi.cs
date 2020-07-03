using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GWowMod
{
    public class CurseForgeApi
    {
        private readonly string _addonsUrl = "https://www.curseforge.com/wow/addons";
        private readonly ICurseForgeClient _curseForgeClient;

        public CurseForgeApi(ICurseForgeClient curseForgeClient)
        {
            _curseForgeClient = curseForgeClient;
        }

        public async Task<IEnumerable<object>> GetAddons(int? limit = null)
        {
            try
            {
                var html = await _curseForgeClient.GetAddons();
                Console.ReadLine();
                Console.WriteLine(html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return null;
        }
    }
}