using System;
using System.Collections.Generic;

namespace PaymentGatewayIntegration.Models;

public partial class Student
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Course { get; set; }

    public int? Age { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
