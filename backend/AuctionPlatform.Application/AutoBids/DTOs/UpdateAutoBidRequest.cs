using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionPlatform.Application.AutoBids.DTOs
{
    public class UpdateAutoBidRequest
    {
        public decimal MaxAmount { get; set; }
        public bool IsActive { get; set; }
    }
}
