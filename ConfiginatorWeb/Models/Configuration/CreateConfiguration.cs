using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Models.Configuration;

public class CreateConfiguration
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string OrganizationPath { get; set; }
    
    [HiddenInput]
    [DataType(DataType.Text)]
    public string? ErrorMessage { get; set; }
}