using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dgt.Extensions.Validation;
using Dgt.MediatR;
using MediatR;

namespace Dgt.CrmMicroservice.Domain.Operations.Branches
{
    public sealed class GetBranchByIdQuery
    {
        public record Request(Guid Id) : IRequest<Response<BranchEntity>>;

        public class Handler : IRequestHandler<Request, Response<BranchEntity>>
        {
            private readonly IBranchRepository _branchRepository;

            public Handler(IBranchRepository branchRepository)
            {
                _branchRepository = branchRepository.WhenNotNull(nameof(branchRepository));
            }

            public async Task<Response<BranchEntity>> Handle(Request request, CancellationToken cancellationToken)
            {
                var queryable = await _branchRepository.GetBranchesAsync(cancellationToken);
                var entities = queryable.Where(branch => branch.Id == request.Id).ToList();

                return entities.Count switch
                {
                    1 => Response.Success(entities.Single()),
                    0 => Response.Success<BranchEntity>(null),
                    _ => throw new InvalidOperationException("Multiple entities with a matching ID were found.")
                };
            }
        }
    }
}