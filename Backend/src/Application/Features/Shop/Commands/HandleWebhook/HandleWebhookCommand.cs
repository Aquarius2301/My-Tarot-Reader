using System;
using MediatR;
using PayOS.Models.Webhooks;

namespace MyTarotReader.Application.Features.Shop;

/// <summary>Request wrapping a raw PayOS webhook payload.</summary>
/// <param name="Webhook">The webhook payload received from PayOS.</param>
public record HandleWebhookCommand(Webhook Webhook) : IRequest<HandleWebhookResult>;

/// <summary>Result of handling a PayOS webhook.</summary>
/// <param name="Processed">Whether a pending order was credited; <c>false</c> for already-handled or ignorable payloads.</param>
public record HandleWebhookResult(bool Processed);
