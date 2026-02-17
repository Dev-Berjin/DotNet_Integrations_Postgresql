using System;
using System.Collections.Generic;

namespace PaymentGatewayIntegration.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string? OrderId { get; set; }

    public string? PaymentId { get; set; }

    public decimal? Amount { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Student Student { get; set; } = null!;
}
