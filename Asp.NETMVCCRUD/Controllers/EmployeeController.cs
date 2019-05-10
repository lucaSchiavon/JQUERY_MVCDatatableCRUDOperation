using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Asp.NETMVCCRUD.Models;
using System.Linq.Dynamic;

namespace Asp.NETMVCCRUD.Controllers
{
    public class EmployeeController : Controller
    {
        #region "CRUD OPERATION WITH JQUERY DATATABLE"
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
                return Json(new { data = empList }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult AddOrEdit(int id = 0)
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
                return Json(new { success = true, message = "deleted succesfully" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion


        #region "JQUERY DATATABLE SERVER SIDE PAGING"

        [HttpGet]
        public ActionResult IndexSSP()
        {         
                    return View();
        }

        [HttpPost]
        public JsonResult CustomServerSideSearchAction(DataTableAjaxPostModel model)
        {
            //**********************
            //una modalità per recuperare i dati in entrata
            //consiste nel recuperare i dati dalla requestform senza usare il model binding
            //nella request.form troviamo tutte le chiavi passate con la richiesta post:

            //if (Request.Form.GetValues("columns[0][search][value]") != null)
            //{
            //    var FiltroNomeContatto = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
            //    var FiltroRicercaGenerica = Request.Form.GetValues("search[value]").FirstOrDefault();
            //}
            //**********************

            // action inside a standard controller
            int filteredResultsCount;
            int totalResultsCount;
            List<Employee> res = YourCustomSearchFunc(model, out filteredResultsCount, out totalResultsCount);

            
            return Json(new
            {
                // this is what datatables wants sending back
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data= res
                //data = result
            }, JsonRequestBehavior.AllowGet);
        }

        public List<Employee> YourCustomSearchFunc(DataTableAjaxPostModel model, out int filteredResultsCount, out int totalResultsCount)
        {
            var searchBy = (model.search != null) ? model.search.value : null;
            var take = model.length;
            var skip = model.start;

            string sortBy = "";
            //bool sortDir = true;
            string sortDir = "asc";

            if (model.order != null)
            {
                // in this example we just default sort on the 1st column
                sortBy = model.columns[model.order[0].column].data;
                //sortDir = model.order[0].dir.ToLower() == "asc";
                sortDir = model.order[0].dir;
            }
            else
            {
                sortBy = model.columns[0].data;
                sortDir ="asc";
            }

            // search the dbase taking into consideration table sorting and paging
            var result = GetDataFromDbase(searchBy, take, skip, sortBy, sortDir, out filteredResultsCount, out totalResultsCount);
            if (result == null)
            {
                // empty collection...
                return new List<Employee>();
            }
            return result;
        }


        public List<Employee> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, out int filteredResultsCount, out int totalResultsCount)
        {
           
            List<Employee> resultTemp = null;
            List<Employee> result = null;
            using (DbModel db = new DbModel())
            {
               
                resultTemp = db.Employee.ToList<Employee>();

                if (!String.IsNullOrEmpty(searchBy))
                {
                    resultTemp = resultTemp.Where(x => x.Name == searchBy).ToList();
                }
                   
                resultTemp = resultTemp.OrderBy(sortBy + " " + sortDir).ToList();
                result = resultTemp.Skip(skip).Take(take).ToList();

            
            }
            //var result = Db.DatabaseTableEntity
            //               .AsExpandable()
            //               .Where(whereClause)
            //               .Select(m => new Employee
            //               {
            //                   Id = m.Id,
            //                   Firstname = m.Firstname,
            //                   Lastname = m.Lastname,
            //                   Address1 = m.Address1,
            //                   Address2 = m.Address2,
            //                   Address3 = m.Address3,
            //                   Address4 = m.Address4,
            //                   Phone = m.Phone,
            //                   Postcode = m.Postcode,
            //               })
            //               .OrderBy(sortBy, sortDir) // have to give a default order when skipping .. so use the PK
            //               .Skip(skip)
            //               .Take(take)
            //               .ToList();

            // now just get the count of items (without the skip and take) - eg how many could be returned with filtering
            filteredResultsCount = resultTemp.Count();
            totalResultsCount = resultTemp.Count();

            return result;
        }
        //private Expression<Func<DatabaseTableMappedClass, bool>> BuildDynamicWhereClause(DBEntities entities, string searchValue)
        //{
        //    // simple method to dynamically plugin a where clause
        //    var predicate = PredicateBuilder.New<DatabaseTableMappedClass>(true); // true -where(true) return all
        //    if (String.IsNullOrWhiteSpace(searchValue) == false)
        //    {
        //        // as we only have 2 cols allow the user type in name 'firstname lastname' then use the list to search the first and last name of dbase
        //        var searchTerms = searchValue.Split(' ').ToList().ConvertAll(x => x.ToLower());

        //        predicate = predicate.Or(s => searchTerms.Any(srch => s.Firstname.ToLower().Contains(srch)));
        //        predicate = predicate.Or(s => searchTerms.Any(srch => s.Lastname.ToLower().Contains(srch)));
        //    }
        //    return predicate;
        //}


        #endregion
    }
}