﻿using Allard.Configinator.Core;
using ConfiginatorWeb.Interactors;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConfiginatorWeb.Controllers;

public class TokenController : Controller
{
    private readonly IMediator _mediator;

    public TokenController(IMediator mediator)
    {
        _mediator = Guards.NotDefault(mediator, nameof(mediator));
    }

    // GET
    public async Task<IActionResult> Index(string tokenSetName, CancellationToken cancellationToken)
    {
        var tokenSet = await _mediator.Send(new TokenSetComposedQuery(tokenSetName), cancellationToken);
        return View(new EditTokenSetView(tokenSet.TokenSet));
    }

    public async Task<IActionResult> EditValue(string tokenSetName, string key, CancellationToken cancellationToken)
    {
        var tokenSet = await _mediator.Send(new TokenSetComposedQuery(tokenSetName), cancellationToken);
        var token =
            tokenSet.TokenSet.Tokens.ContainsKey(key)
                ? tokenSet.TokenSet.Tokens[key].Token
                : JToken.Parse("\"\"");

        return View(new EditValueView
        {
            TokenSetName = tokenSetName,
            Key = key,
            SelectedToken = token,
            TokensComposed = tokenSet.TokenSet
        });
    }

    [HttpPost]
    public async Task<IActionResult> SaveValue(string tokenSetName, string key, string value,
        CancellationToken cancellationToken)
    {
        var command = new SetTokenValueCommand(tokenSetName, key, value);
        await _mediator.Send(command, cancellationToken);
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

public record EditTokenSetView(TokenSetComposedDto TokenSet);

public class EditValueView
{
    public string TokenSetName { get; set; }

    public string Key { get; set; }

    public JToken SelectedToken { get; set; }

    public TokenSetComposedDto TokensComposed { get; set; }
}

public class SaveTokenValue
{
    public string TokenSetName { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}