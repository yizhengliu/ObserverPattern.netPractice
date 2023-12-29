using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildingSurveillanceSystemApplication
{
    internal class Program
    {
        static void Main(string[] args)
        {
        }
    }

    public abstract class Observer : IObserver<ExternalVisitor>
    {
        //
        IDisposable _cancellation;
        protected List<ExternalVisitor> _externalVisitors = new List<ExternalVisitor>();

        public abstract void OnCompleted();

        public abstract void OnError(Exception error);

        public abstract void OnNext(ExternalVisitor value);

        public void Subscribe(IObservable<ExternalVisitor> provider)
        {
            _cancellation = provider.Subscribe(this);
        }

        public void Unsubscribe()
        {
            _cancellation.Dispose();
            _externalVisitors.Clear();
        }
    }

    public class SecurityNotify : Observer
    {

        public override void OnCompleted()
        {
            string heading = "Security Daily Visitor's report";
            Console.WriteLine();

            Console.WriteLine(heading);
            Console.WriteLine(new string('-', heading.Length));
            Console.WriteLine();

            foreach (var externalVistor in _externalVisitors)
            {
                //all vistors have left the building
                externalVistor.InBuilding = false;

                Console.WriteLine($"{externalVistor.Id,-6}{externalVistor.FirstName,-15}{externalVistor.LastName,-15}{externalVistor.EntryTime.ToString("dd MMM yyyy hh:mm:ss"),-25}{externalVistor.ExitDateTime.ToString("dd MMM yyyy hh:mm:ss"),-25}");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        public override void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public override void OnNext(ExternalVisitor value)
        {
            var externalVisitor = value;

            var externalVistorListItem = _externalVisitors.FirstOrDefault(e => e.Id == externalVisitor.Id);
            if (externalVistorListItem == null)
            {
                _externalVisitors.Add(externalVisitor);
                Console.WriteLine($"Security notificationL: VistorId({externalVisitor.Id}), First Name({externalVisitor.FirstName}), Last Name({externalVisitor.LastName}), entered the building, DateTime({externalVisitor.EntryTime.ToString("dd MMM yyyy hh:mm:ss")})");
                Console.WriteLine();
            }
            else 
            {
                if (externalVisitor.InBuilding == false) 
                {
                    externalVistorListItem.InBuilding = false;
                    externalVistorListItem.ExitDateTime = externalVisitor.ExitDateTime;
                    Console.WriteLine($"Security notificationL: VistorId({externalVisitor.Id}), First Name({externalVisitor.FirstName}), Last Name({externalVisitor.LastName}), entered the building, DateTime({externalVisitor.ExitDateTime.ToString("dd MMM yyyy hh:mm:ss")})");
                    Console.WriteLine();
                }
            }
        }
    }

    public class Employee : IEmployee
    {
        public int Id { get; set ; }
        public string FirstName { get ; set ; }
        public string LastName { get; set; }
        public string JobTitle { get; set; }
    }

    public interface IEmployee 
    {
        int Id { get; set; }
        string FirstName {  get; set; }
        string LastName { get; set; }
        string JobTitle { get; set; }
    }

    public class EmployeeNotify : Observer
    {
        IEmployee _employee = null;
        
        public EmployeeNotify(IEmployee employee) 
        {
            _employee = employee;
        }

        public override void OnCompleted()
        {
            string heading = $"{_employee.FirstName + " " + _employee.LastName} Daily Visitor's report";
            Console.WriteLine();

            Console.WriteLine(heading);
            Console.WriteLine(new string('-', heading.Length));
            Console.WriteLine();

            foreach(var externalVistor in _externalVisitors) 
            {
                //all vistors have left the building
                externalVistor.InBuilding = false;

                Console.WriteLine($"{externalVistor.Id,-6}{externalVistor.FirstName,-15}{externalVistor.LastName,-15}{externalVistor.EntryTime.ToString("dd MMM yyyy hh:mm:ss"),-25}{externalVistor.ExitDateTime.ToString("dd MMM yyyy hh:mm:ss"),-25}");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        public override void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public override void OnNext(ExternalVisitor value)
        {
            var externalVisitor = value;
            if(externalVisitor.EmployeeContactId == _employee.Id) 
            {
                var externalVisitorListItem = _externalVisitors.FirstOrDefault(e => e.Id == externalVisitor.Id);

                //if first time visit
                if (externalVisitorListItem == null)
                {
                    _externalVisitors.Add(externalVisitor);
                    Console.WriteLine($"{_employee.FirstName + " " + _employee.LastName}, your visitor has arrived. Visitor Id({externalVisitor.Id}), First Name({externalVisitor.FirstName}), Last Name({externalVisitor.LastName}), entered the building, DateTime({externalVisitor.EntryTime.ToString("dd MMM yyyy hh:mm:ss")})");
                    Console.WriteLine();
                }
                else 
                {
                    // if already gone
                    if (externalVisitor.InBuilding == false)
                    {
                        externalVisitorListItem.InBuilding = false;
                        externalVisitorListItem.ExitDateTime = externalVisitor.ExitDateTime;
                    }
                }
            }
        }
    }

    //public class 

    public class Unsubscriber<ExternalVisitor> : IDisposable
    {
        public List<IObserver<ExternalVisitor>> _observers;
        private IObserver<ExternalVisitor> _observer;
        public Unsubscriber(List<IObserver<ExternalVisitor>> observers, IObserver<ExternalVisitor> observer) 
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if(_observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }

    public class SecuritySurveillanceHub : IObservable<ExternalVisitor>
    {
        private List<ExternalVisitor> _externalVisitors;
        private List<IObserver<ExternalVisitor>> _observers;



        public IDisposable Subscribe(IObserver<ExternalVisitor> observer)
        {
            if (_observers.Contains(observer)) 
            {
                _observers.Add(observer);
            }

            foreach (var externalVistor in _externalVisitors) 
            {
                observer.OnNext(externalVistor);
            }

            return new Unsubscriber<ExternalVisitor>(_observers, observer);
        }

        public void ConfirmExternalVisiterEntersBuilding(int id, string firstName,string lastName, string companyName, string jobTitle, DateTime enterDateTime, int employeeContactId)
        {
            ExternalVisitor externalVisitor = new ExternalVisitor 
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                CompanyName = companyName,
                JobTitle = jobTitle,
                EntryTime = enterDateTime,
                EmployeeContactId = employeeContactId,
                InBuilding = true
            };
            _externalVisitors.Add(externalVisitor);

            foreach (var observer in _observers) 
            {
                observer.OnNext(externalVisitor);
            }
        }

        public void ConfirmExternalVisitorExitsBuilding(int externalVisitorId, DateTime exitDateTime) 
        {
            var externalVisitor = _externalVisitors.FirstOrDefault(e => e.Id == externalVisitorId);

            if (externalVisitor != null) 
            {
                externalVisitor.ExitDateTime = exitDateTime;
                externalVisitor.InBuilding = false;
                foreach (var observer in _observers) 
                {
                    observer.OnNext(externalVisitor);
                }
            }
        }

        public void BuildingEntryCutOffTimeReached() 
        {
            if (_externalVisitors.Where(e => e.InBuilding == true).ToList().Count == 0) 
            {
                foreach(var observer in _observers) 
                {
                    observer.OnCompleted();
                }
            }
        }
    }


    public class ExternalVisitor 
    {
        public int Id {  get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName {  get; set; }
        public string JobTitle { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime ExitDateTime { get; set;}
        public int EmployeeContactId {  get; set; }
        public bool InBuilding {  get; set; }
    }

    
}
