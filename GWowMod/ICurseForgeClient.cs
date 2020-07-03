using Refit;
using System.Threading.Tasks;

namespace GWowMod
{
    public interface ICurseForgeClient
    {
        [Get("/wow/addons?__cf_chl_captcha_tk__=4b9a378d41d425431e3aa433b96a271304349aac-1593118533-0-Aakw-oQl3OJoLi7cSDzE-UwD1w7wr-pse8MIyjEp4fw-kHMrGVzWL3EOtoWbvEZ4P9i5e6xcRBjNLkupq7xvlA4whxMzxZyVJBJIRCdKmv1hLaDA2xd7yYXPHyqRtbKWpnf1_ggj1iWvqoKwG2XCEOCgb-bnWWRhOjaYBAH09dPYUROnxS20Ivw5XC4r-GmHDStDEf5pKsXdF-Mg71rOskVLqKNrjTvUvGJQ5_AEcEFTCL_lMyAXe3XO38gYivTJ3KIMoc0Iv1DbK5B8blis-YzfFtlDioBEKNJLcNlQrUgaizNTSU_kT5KTXNa3o-ZfdKmUHApsa4lHgfX3fUZtvS4H3hamLX2D-nRkicTRE3rxYe-QA9d9ftiXFZArzui0Ldk0WvyT-GysPYIVMPqroSUFrWzkvzWQY5EJee7X3KTsa_nkGqWaQyTT_LNHXzfNF5OdZwj4nuXzROJL_7tX8nBTQiavP7RKSjJYHtJG7QnRnEI4Y68KcOo7pKRdjiy_BaiLcW0Alhq81l1tnm6qQWJ9gWGAutX1eqn9PIvXVR2TN6ZsH1Xo6gRukzs5t4oFdA")]
        Task<string> GetAddons(int? limit = null);
    }
}