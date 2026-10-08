namespace Marraia.POC.Application.Services;

public interface IQuoteService
{
    Task<decimal> GetDollarAsync(CancellationToken cancellationToken = default);
}
