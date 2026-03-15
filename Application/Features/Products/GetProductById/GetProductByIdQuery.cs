using Application.Shared.DTOs;
using MediatR;

namespace Application.Features.Products.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;
