using MeetingScheduler.Api.Dtos;

namespace MeetingScheduler.Api.Services;

public interface IBookingService
{
    Task<IReadOnlyList<BookingInstanceDto>> GetBookingsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<CreateBookingResponse> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task DeleteBookingAsync(Guid id, bool deleteSeries = false, CancellationToken cancellationToken = default);
}
