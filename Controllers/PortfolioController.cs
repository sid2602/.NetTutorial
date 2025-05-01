using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Extensions;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/portfolio")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IStockRepository _stockRepo;
        private readonly IPorfolioRepository _portRepo;
        public PortfolioController(
            UserManager<AppUser> userManager,
            IStockRepository stockRepo,
            IPorfolioRepository portRepo
            )
        {
            _userManager = userManager;
            _stockRepo = stockRepo;
            _portRepo = portRepo;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPortfolio(){
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);
            var userPortfolio = await _portRepo.GetUserPortfolio(appUser);
            return Ok(userPortfolio);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPortfolio(string symbol){
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);
            var stock = await _stockRepo.GetBySymbolAsync(symbol);

            if(stock == null) return BadRequest("Stock not found");

            var userPortfolio = await _portRepo.GetUserPortfolio(appUser);

            if(userPortfolio.Any(e => e.Symbol.ToLower() == symbol.ToLower())) return BadRequest("cannot add the same stock to portfolio");

            var portfolioModel = new Portfolio{
                StockId = stock.Id,
                AppUserId = appUser.Id
            };

            await _portRepo.CreateAsync(portfolioModel);
            if(portfolioModel == null){
                return StatusCode(500, "Could not create");
            }else{
                return Created();
            }
        }
    }
}