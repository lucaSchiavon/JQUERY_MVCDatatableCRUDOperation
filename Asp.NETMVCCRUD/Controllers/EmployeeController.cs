using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Asp.NETMVCCRUD.Models;

namespace Asp.NETMVCCRUD.Controllers
{
    public class EmployeeController : Controller
    {
        // GET: Employee
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetData()
        {
            using (DbModel db = new DbModel())
            {
                List<Employee> empList = db.Employee.ToList<Employee>();
                return Json(new { data = empList },JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult AddOrEdit(int id=0)
        {
            if (id == 0)
            {
                return View(new Employee());
            }
            else
            {
                using (DbModel db = new DbModel())
                {
                    Employee emp = db.Employee.Where(x => x.EmployeId == id).FirstOrDefault<Employee>();
                    return View(emp);
                }
            }
           
        }
        [HttpPost]
        public ActionResult AddOrEdit(Employee emp)
        {
            using (DbModel db = new DbModel())
            {
                if (emp.EmployeId == 0)
            {
               
                    db.Employee.Add(emp);
                    db.SaveChanges();
                    return Json(new { success = true, message = "save succesfully" });
               
            }
            else
            {
                    db.Entry(emp).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { success = true, message = "update succesfully" });
                }
       }



        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            using (DbModel db = new DbModel())
            {
                Employee emp = db.Employee.Where(x => x.EmployeId == id).FirstOrDefault<Employee>();
                db.Employee.Remove(emp);
                db.SaveChanges();
                return Json(new { success = true, message = "deleted succesfully" } , JsonRequestBehavior.AllowGet);
            }

        }
    }
}