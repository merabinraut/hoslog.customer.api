using hoslog.customer.shared.LocationManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.domain.LocationManagement
{
    public interface ILocationManagementRepository
    {
        Task<List<LocationCommon>> GetLocationList();
    }
}
