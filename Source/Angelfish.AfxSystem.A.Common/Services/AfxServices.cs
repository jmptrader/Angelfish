using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angelfish.AfxSystem.A.Common.Services
{
    public class AfxServices : IServiceProvider
    {
        private Dictionary<Type, object> _mapServices = new Dictionary<Type, object>();

        public object GetService(Type serviceType)
        {
            if (serviceType != null)
            {
                if (_mapServices.ContainsKey(serviceType))
                {
                    return _mapServices[serviceType];
                }
                else
                {
                    var msg = string.Format("The service \"{0}\" has not been registered.", serviceType.FullName);
                    throw new ArgumentOutOfRangeException(msg);
                }
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        public void AddService(Type serviceType, object serviceImpl)
        {
            if ((serviceType != null) && (serviceImpl != null))
            {
                if (!_mapServices.ContainsKey(serviceType))
                {
                    _mapServices.Add(serviceType, serviceImpl);
                }
                else
                {
                    var msg = string.Format("The service \"{0}\" has already been registered.", serviceType.FullName);
                    throw new ArgumentOutOfRangeException(msg);
                }
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
    }
}
