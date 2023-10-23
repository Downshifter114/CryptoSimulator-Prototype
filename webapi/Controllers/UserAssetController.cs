﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.Data;
using webapi.Models;


namespace webapi.Controllers
{
    [Route("/api")]
    [ApiController]

    public class UserAssetController : ControllerBase
    {
        private readonly DataContext _db;

        public UserAssetController(DataContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("{userId}")]

        public async Task<ActionResult<IEnumerable<UserAsset>>> GetUserAsset(int userId)
        {
            var userAssets = await _db.userAssets
                .Where(x => x.UserId == userId)
                .Select(x => new { CoinCode = x.CoinCode, Amount = x.Amount })
                .ToListAsync();

            return Ok(userAssets);
        }

        [HttpPost]

        [Route("BuyCoin")]
        public async Task<ActionResult<string>> BuyCoin([FromBody] CoinPurchaseRequest request)
        {
            try
            {
                var user =await _db.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId);

                if (user != null && user.Balance >= request.Amount * request.CurrentPrice) 
                {
                    var existingUserAsset = _db.userAssets
                       .FirstOrDefault(a => a.UserId == request.UserId && a.CoinCode == request.CoinCode);

                    if (existingUserAsset != null)
                    {
                        existingUserAsset.Amount += request.Amount;
                    }
                    else
                    {
                        var newUserAsset = new UserAsset
                        {
                            UserId = request.UserId,
                            CoinCode = request.CoinCode,
                            Amount = request.Amount,
                            User = user
                        };

                        _db.userAssets.Add(newUserAsset);
                    }

                    user.Balance -= request.Amount * request.CurrentPrice;

                    var purchase = new CoinPurchaseRequest
                    {
                        UserId = request.UserId,
                        CoinCode = request.CoinCode,
                        Amount = request.Amount,
                        CurrentPrice = request.CurrentPrice,
                        PurchaseDateTime = request.PurchaseDateTime,
                    };

                    _db.coinPurchaseRequests.Add(purchase);

                    _db.SaveChanges();

                    return Ok(new { message = $"Success! Coin purchased successfully. Current balance: ${user.Balance}" });
                }

                else
                {
                    return BadRequest(new { message = "Insufficient balance!" });
                }
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }


        }
        [HttpGet]
        [Route("Balance")]

        public IActionResult GetCurrentBalance(int id)
        {
            var user = _db.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }
            return Ok(user.Balance);
        }

        
    }


}
