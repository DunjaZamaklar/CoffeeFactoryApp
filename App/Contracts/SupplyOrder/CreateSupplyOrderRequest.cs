﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using App.Data.Entities;

namespace App.Contracts;
public class CreateSupplyOrderRequest
{
    public Guid EmployeeContractId { get; set; }
    public Guid StatusId { get; set; }
}