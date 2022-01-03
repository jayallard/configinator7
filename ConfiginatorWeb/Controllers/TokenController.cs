﻿using Configinator7.Core;
using Configinator7.Core.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConfiginatorWeb.Controllers;

public class TokenController : Controller
{
    private readonly SuperAggregate _aggregate;

    public TokenController(SuperAggregate aggregate)
    {
        _aggregate = aggregate;
    }

    // GET
    public IActionResult Index(string tokenSetName)
    {
        var tokens = _aggregate.ResolveTokenSet(tokenSetName);
        if (tokens == null) throw new ArgumentException("make me a 404!");

        var v = new EditTokenSetView
        {
            Resolved = tokens
        };
        return View(v);
    }

    public IActionResult EditValue(string tokenSetName, string key)
    {
        var tokenSet = _aggregate.TemporaryExposureTokenSets[tokenSetName];
        var token =
            tokenSet.Tokens.ContainsKey(key)
                ? tokenSet.Tokens[key]
                : JToken.Parse("\"\"");
        
        var resolved = _aggregate.ResolveTokenSet(tokenSetName);
        if (resolved == null)
        {
            // todo: 404
            throw new NotImplementedException();
        }
        
        return View(new EditValueView
        {
            TokenSetName = tokenSetName,
            Key = key,
            SelectedToken = token,
            TokensResolved = resolved
        });
    }

    [HttpPost]
    public IActionResult SaveValue(string tokenSetName, string key, string value)
    {
        var jsonValue = ToToken();
        _aggregate.SetTokenValue(tokenSetName, key, jsonValue);
        return RedirectToAction("index", new {tokenSetName = tokenSetName});

        JToken ToToken()
        {
            // hack - need to be explicit about type
            try
            {
                return JToken.Parse(value);
            }
            catch (JsonReaderException)
            {
                return value;
            }
        }
    }
}

public class EditTokenSetView
{
    public TokenSetResolved Resolved { get; set; }
}

public class EditValueView
{
    public string TokenSetName { get; set; }
    
    public string Key { get; set; }

    public JToken SelectedToken { get; set; }
    
    public TokenSetResolved TokensResolved { get; set; }
}

public class SaveTokenValue
{
    public string TokenSetName { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}