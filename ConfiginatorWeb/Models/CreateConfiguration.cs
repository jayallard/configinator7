﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Models;

public class CreateConfiguration
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Path { get; set; }
    
    [DataType(DataType.MultilineText)]
    public string? Schema { get; set; }
    
    [HiddenInput]
    [DataType(DataType.Text)]
    public string? ErrorMessage { get; set; }
}