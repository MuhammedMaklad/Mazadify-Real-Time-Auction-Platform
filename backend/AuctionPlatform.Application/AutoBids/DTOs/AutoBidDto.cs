using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionPlatform.Application.AutoBids.DTOs
{
    public class AutoBidDto
    {
        public Guid Id { get; set; }

        public Guid AuctionId { get; set; }

        public decimal MaxAmount { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
