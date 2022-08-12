using GroupDocs.Viewer;
using GroupDocs.Viewer.Options;
using SimpleProject.Models;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleProject.Controllers
{
    public class UserController : Controller
    {
        // Connection string config
        readonly static string ConnectionString = ConfigurationManager.ConnectionStrings["sqlConfig"].ConnectionString;

        // config (KATASQL)
        SqlConnection connection = new SqlConnection(ConnectionString);

        // ado.net Entity framework
        private mvc_dbContext db = new mvc_dbContext();

        // GET: User
        [HttpGet]
        public ActionResult Index()
        {
            var compiler = new SqlServerCompiler();
            var dbmvc = new QueryFactory(connection, compiler);

            // GetAll
            var queryGetAll = dbmvc.Query("mvc_tb");
            SqlResult result = compiler.Compile(queryGetAll);
            string sqlGetAll = result.Sql;
            List<object> bindings = result.Bindings;

            DataTable userDT = new DataTable(); // Declare DT  

            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                sqlCon.Open();
                SqlDataAdapter myAdapter = new SqlDataAdapter(sqlGetAll, sqlCon);
                myAdapter.Fill(userDT);
                sqlCon.Close();
            }
            return View(userDT);
        }

        // GET: User/Create
        // Action method to Return a form to add users
        [HttpGet]
        public ActionResult Create()
        {
            var Titles = new List<string>() { "Mr", "Mrs", "Miss" };
            ViewBag.Titles = Titles;
            return View(new UserModel());
        }

        // POST: User/Create
        // Action method to create user
        [HttpPost]
        public ActionResult Create(UserModel userModel, HttpPostedFileBase DocsFile, HttpPostedFileBase ImgFile)
        {
            var compilerCreate = new SqlServerCompiler();
            var createQF = new QueryFactory(connection, compilerCreate);

            // Insert into table 1 Query(Query 1)
            var colsT1 = new[] { "Title", "FirstName", "LastName", "Address", "Email", "MobNum" };
            var dataT1 = new[] { userModel.Title, userModel.FirstName, userModel.LastName, userModel.Address, userModel.Email, userModel.MobNum };
            var queryCreate = createQF.Query("mvc_tb").AsInsert(colsT1, dataT1);
            SqlResult createSR = compilerCreate.Compile(queryCreate);
            string sqlCreate = createSR.Sql;
            List<object> bindings = createSR.Bindings;

            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                sqlCon.Open();
                // Query 1 execution 
                SqlCommand sqlCmd = new SqlCommand(sqlCreate, sqlCon);
                sqlCmd.Parameters.AddWithValue("@p0", userModel.Title);
                sqlCmd.Parameters.AddWithValue("@p1", userModel.FirstName);
                sqlCmd.Parameters.AddWithValue("@p2", userModel.LastName);
                sqlCmd.Parameters.AddWithValue("@p3", userModel.Address);
                sqlCmd.Parameters.AddWithValue("@p4", userModel.Email);
                sqlCmd.Parameters.AddWithValue("@p5", userModel.MobNum);
                sqlCmd.ExecuteNonQuery();
                sqlCmd.Parameters.Clear();
            }

            if ((DocsFile == null) && (ImgFile == null))
            {
                //Get id
                var userId = createQF.Query("mvc_tb").SelectRaw("MAX(id)");
                SqlResult userIdQ = compilerCreate.Compile(userId);
                string sqlCreateQ = userIdQ.Sql;
                List<object> bindingsQ = userIdQ.Bindings;
                // Insert into table 2 Query (Query 2)
                var colsT2 = new[] { "userId" };
                var dataT2 = new[] { userModel.userId.ToString() };
                var queryT2 = createQF.Query("documents").AsInsert(colsT2, dataT2);
                SqlResult T2SR = compilerCreate.Compile(queryT2);
                string sqlT2 = T2SR.Sql;
                List<object> bindingsT2 = T2SR.Bindings;

                using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
                {
                    sqlCon.Open();
                    //Get Id query
                    SqlCommand GetIdCmd = new SqlCommand(userIdQ.ToString(), sqlCon);
                    string valID = (string)GetIdCmd.ExecuteScalar().ToString();
                    // Query 1 execution 
                    SqlCommand sqlCmdId = new SqlCommand(sqlT2, sqlCon);
                    sqlCmdId.Parameters.AddWithValue("@p0", valID);
                    sqlCmdId.ExecuteNonQuery();
                }
            }

            else if ((ImgFile != null) && (DocsFile == null))
            {
                // Images upload codes
                string imgName = Path.GetFileNameWithoutExtension(userModel.ImgFile.FileName);
                string extentionImg = Path.GetExtension(userModel.ImgFile.FileName);
                var FnameImg = userModel.FirstName + userModel.LastName;
                imgName = imgName + FnameImg + extentionImg;
                userModel.ImgPath = "~/App_Data/Image/" + imgName;
                imgName = Path.Combine(Server.MapPath("~/App_Data/Image/"), imgName);
                userModel.ImgFile.SaveAs(imgName);
                //Get id
                var userId = createQF.Query("mvc_tb").SelectRaw("MAX(id)");
                SqlResult userIdQ = compilerCreate.Compile(userId);
                string sqlCreateQ = userIdQ.Sql;
                List<object> bindingsQ = userIdQ.Bindings;
                // Insert into table 2 Query (Query 2)
                var colsT2 = new[] { "ImgType", "ImgPath", "userId" };
                var dataT2 = new[] { extentionImg, userModel.ImgPath, userModel.userId.ToString() };
                var queryT2 = createQF.Query("documents").AsInsert(colsT2, dataT2);
                SqlResult T2SR = compilerCreate.Compile(queryT2);
                string sqlT2 = T2SR.Sql;
                List<object> bindingsT2 = T2SR.Bindings;
                using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
                {
                    sqlCon.Open();
                    //Get Id query
                    SqlCommand GetIdCmd = new SqlCommand(userIdQ.ToString(), sqlCon);
                    string valID = (string)GetIdCmd.ExecuteScalar().ToString();
                    // Query 1 execution 
                    SqlCommand sqlCmdId = new SqlCommand(sqlT2, sqlCon);
                    sqlCmdId.Parameters.AddWithValue("@p0", extentionImg);
                    sqlCmdId.Parameters.AddWithValue("@p1", userModel.ImgPath);
                    sqlCmdId.Parameters.AddWithValue("@p2", valID);
                    sqlCmdId.ExecuteNonQuery();
                }
            }

            else if ((DocsFile != null) && (ImgFile == null))
            {
                foreach (HttpPostedFileBase files in userModel.DocsFile)
                {
                    // Docs upload codes
                    string fileName = Path.GetFileNameWithoutExtension(files.FileName);
                    string extention = Path.GetExtension(files.FileName);
                    var FnameDocs = userModel.FirstName + userModel.LastName;
                    fileName = fileName + FnameDocs + extention;
                    userModel.DocsPath = "~/App_Data/File/" + fileName;
                    fileName = Path.Combine(Server.MapPath("~/App_Data/File/"), fileName);
                    files.SaveAs(fileName);
                    //Get id
                    var userId = createQF.Query("mvc_tb").SelectRaw("MAX(id)");
                    SqlResult userIdQ = compilerCreate.Compile(userId);
                    string sqlCreateQ = userIdQ.Sql;
                    List<object> bindingsQ = userIdQ.Bindings;
                    // Insert into table 2 Query (Query 2)
                    var colsT2 = new[] { "DocsType", "DocsPath", "userId" };
                    var dataT2 = new[] { extention, userModel.DocsPath, userModel.userId.ToString() };
                    var queryT2 = createQF.Query("documents").AsInsert(colsT2, dataT2);
                    SqlResult T2SR = compilerCreate.Compile(queryT2);
                    string sqlT2 = T2SR.Sql;
                    List<object> bindingsT2 = T2SR.Bindings;

                    using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
                    {
                        sqlCon.Open();
                        //Get Id query
                        SqlCommand GetIdCmd = new SqlCommand(userIdQ.ToString(), sqlCon);
                        string valID = (string)GetIdCmd.ExecuteScalar().ToString();
                        // Query 1 execution 
                        SqlCommand sqlCmdId = new SqlCommand(sqlT2, sqlCon);
                        sqlCmdId.Parameters.AddWithValue("@p0", extention);
                        sqlCmdId.Parameters.AddWithValue("@p1", userModel.DocsPath);
                        sqlCmdId.Parameters.AddWithValue("@p2", valID);
                        sqlCmdId.ExecuteNonQuery();
                    }
                }
            }

            else if ((ImgFile != null) && (DocsFile != null))
            {
                foreach (HttpPostedFileBase files in userModel.DocsFile)
                {
                    // Docs upload codes
                    string fileName = Path.GetFileNameWithoutExtension(files.FileName);
                    string extention = Path.GetExtension(files.FileName);
                    var FnameDocs = userModel.FirstName + userModel.LastName;
                    fileName = fileName + FnameDocs + extention;
                    userModel.DocsPath = "~/App_Data/File/" + fileName;
                    fileName = Path.Combine(Server.MapPath("~/App_Data/File/"), fileName);
                    files.SaveAs(fileName);
                    // Images upload codes
                    string imgName = Path.GetFileNameWithoutExtension(userModel.ImgFile.FileName);
                    string extentionImg = Path.GetExtension(userModel.ImgFile.FileName);
                    var FnameImg = userModel.FirstName + userModel.LastName;
                    imgName = imgName + FnameImg + extentionImg;
                    userModel.ImgPath = "~/App_Data/Image/" + imgName;
                    imgName = Path.Combine(Server.MapPath("~/App_Data/Image/"), imgName);
                    userModel.ImgFile.SaveAs(imgName);
                    // Insert into table 2 Query (Query 2)
                    var colsT2 = new[] { "DocsType", "DocsPath", "ImgType", "ImgPath", "userId" };
                    var dataT2 = new[] { extention, userModel.DocsPath, extentionImg, userModel.ImgPath, userModel.userId.ToString() };
                    var queryT2 = createQF.Query("documents").AsInsert(colsT2, dataT2);
                    SqlResult T2SR = compilerCreate.Compile(queryT2);
                    string sqlT2 = T2SR.Sql;
                    List<object> bindingsT2 = T2SR.Bindings;
                    //Get id
                    var userId = createQF.Query("mvc_tb").SelectRaw("MAX(id)");
                    SqlResult userIdQ = compilerCreate.Compile(userId);
                    string sqlCreateQ = userIdQ.Sql;
                    List<object> bindingsQ = userIdQ.Bindings;

                    using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
                    {
                        sqlCon.Open();
                        //Get Id query
                        SqlCommand GetIdCmd = new SqlCommand(userIdQ.ToString(), sqlCon);
                        string valID = (string)GetIdCmd.ExecuteScalar().ToString();
                        // Query 1 execution 
                        SqlCommand sqlCmdId = new SqlCommand(sqlT2, sqlCon);
                        sqlCmdId.Parameters.AddWithValue("@p0", extention);
                        sqlCmdId.Parameters.AddWithValue("@p1", userModel.DocsPath);
                        sqlCmdId.Parameters.AddWithValue("@p2", extentionImg);
                        sqlCmdId.Parameters.AddWithValue("@p3", userModel.ImgPath);
                        sqlCmdId.Parameters.AddWithValue("@p4", valID);
                        sqlCmdId.ExecuteNonQuery();
                    }
                }
            }

            return RedirectToAction("Index");
        }

        // Preview docs [InProgress]
        [HttpGet]
        public ActionResult ViewDocument()
        {
            string fileName = Request.Form["fileToView"];
            string outputDirectory = ("Output/");
            string OutputFilePath = Path.Combine(outputDirectory, "output.pdf");
            using (Viewer viewer = new Viewer(fileName))
            {
                PdfViewOptions options = new PdfViewOptions(OutputFilePath);
                viewer.View(options);
            }
            var fileStream = new FileStream("Output/" + "output.pdf",
                FileMode.Open,
                FileAccess.Read);
            var fsResult = new FileStreamResult(fileStream, "application/pdf");
            return fsResult;
        }

        // GET: User/Edit/5
        public ActionResult Edit(int id)
        {
            var Titles = new List<string>() { "Mr", "Mrs", "Miss" };
            ViewBag.Titles = Titles;

            UserModel userModel = new UserModel();
            DataTable user = new DataTable();

            var compilerEdit = new SqlServerCompiler();
            var EditQF = new QueryFactory(connection, compilerEdit);

            var queryEdit = EditQF.Query("mvc_tb").Where("id", "@Id").Join("documents", "mvc_tb.id", "documents.userId");

            SqlResult EditSR = compilerEdit.Compile(queryEdit);
            string sqlEdit = EditSR.Sql;
            List<object> bindings = EditSR.Bindings;

            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                sqlCon.Open();
                SqlDataAdapter myAdapter = new SqlDataAdapter(sqlEdit, sqlCon);
                myAdapter.SelectCommand.Parameters.AddWithValue("@p0", id);
                myAdapter.Fill(user);
            }

            if (user.Rows.Count == 1 || user.Rows.Count > 1)
            {
                userModel.Id = Convert.ToInt32(user.Rows[0][0].ToString());
                userModel.Title = user.Rows[0][1].ToString();
                userModel.FirstName = user.Rows[0][2].ToString();
                userModel.LastName = user.Rows[0][3].ToString();
                userModel.Address = user.Rows[0][4].ToString();
                userModel.Email = user.Rows[0][5].ToString();
                userModel.MobNum = user.Rows[0][6].ToString();
                userModel.ImgPath = user.Rows[0][7].ToString();
                userModel.DocsPath = user.Rows[0][8].ToString();

                return View(userModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        // POST: User/Edit/5
        [HttpPost]
        public ActionResult Edit(UserModel userModel, HttpPostedFileBase DocsFile, HttpPostedFileBase ImgFile)
        {
            // Title condition
            if (userModel.Title != null)
            {
                var compilerCreate = new SqlServerCompiler();
                var createQF = new QueryFactory(connection, compilerCreate);
                // Update in table 1 (Query 1)
                var cols = new[] { "Title", "FirstName", "LastName", "Address", "Email", "MobNum" };
                var data = new[] { userModel.Title, userModel.FirstName, userModel.LastName, userModel.Address, userModel.Email, userModel.MobNum };
                var queryCreate = createQF.Query("mvc_tb").AsUpdate(cols, data).Where("id", "@Id");
                SqlResult EditSR = compilerCreate.Compile(queryCreate);
                string sqlEditS = EditSR.Sql;
                List<object> bindings = EditSR.Bindings;

                using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
                {
                    sqlCon.Open();
                    // Query 1 execution 
                    SqlDataAdapter myAdapter = new SqlDataAdapter(sqlEditS, sqlCon);
                    SqlCommand sqlCmd = new SqlCommand(sqlEditS, sqlCon);
                    sqlCmd.Parameters.AddWithValue("@p6", userModel.Id);
                    sqlCmd.Parameters.AddWithValue("@p0", userModel.Title);
                    sqlCmd.Parameters.AddWithValue("@p1", userModel.FirstName);
                    sqlCmd.Parameters.AddWithValue("@p2", userModel.LastName);
                    sqlCmd.Parameters.AddWithValue("@p3", userModel.Address);
                    sqlCmd.Parameters.AddWithValue("@p4", userModel.Email);
                    sqlCmd.Parameters.AddWithValue("@p5", userModel.MobNum);
                    sqlCmd.ExecuteNonQuery();
                    sqlCmd.Parameters.Clear();
                }
            }

            if (userModel.Title == null)
            {
                var compilerCreate = new SqlServerCompiler();
                var createQF = new QueryFactory(connection, compilerCreate);
                // Update in table 1 (Query 1)
                var cols = new[] { "FirstName", "LastName", "Address", "Email", "MobNum" };
                var data = new[] { userModel.FirstName, userModel.LastName, userModel.Address, userModel.Email, userModel.MobNum };
                var queryCreate = createQF.Query("mvc_tb").AsUpdate(cols, data).Where("id", "@Id");
                SqlResult EditSR = compilerCreate.Compile(queryCreate);
                string sqlEditS = EditSR.Sql;
                List<object> bindings = EditSR.Bindings;

                using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
                {
                    sqlCon.Open();
                    // Query 1 execution 
                    SqlDataAdapter myAdapter = new SqlDataAdapter(sqlEditS, sqlCon);
                    SqlCommand sqlCmd = new SqlCommand(sqlEditS, sqlCon);
                    sqlCmd.Parameters.AddWithValue("@p5", userModel.Id);
                    sqlCmd.Parameters.AddWithValue("@p0", userModel.FirstName);
                    sqlCmd.Parameters.AddWithValue("@p1", userModel.LastName);
                    sqlCmd.Parameters.AddWithValue("@p2", userModel.Address);
                    sqlCmd.Parameters.AddWithValue("@p3", userModel.Email);
                    sqlCmd.Parameters.AddWithValue("@p4", userModel.MobNum);
                    sqlCmd.ExecuteNonQuery();
                    sqlCmd.Parameters.Clear();
                }
            }

            // Docs condition
            if ((ImgFile != null) && (DocsFile == null))
            {
                var compilerCreate = new SqlServerCompiler();
                var createQF = new QueryFactory(connection, compilerCreate);
                // Images upload codes
                string imgName = Path.GetFileNameWithoutExtension(userModel.ImgFile.FileName);
                string extentionImg = Path.GetExtension(userModel.ImgFile.FileName);
                var FnameImg = userModel.FirstName + userModel.LastName;
                imgName = imgName + FnameImg;
                userModel.ImgPath = "~/App_Data/Image/" + imgName;
                imgName = Path.Combine(Server.MapPath("~/App_Data/Image/"), imgName);
                userModel.ImgFile.SaveAs(imgName);
                // Update in table 2 (Query 2)
                var colsT2 = new[] { "ImgType", "ImgPath" };
                var dataT2 = new[] { extentionImg, userModel.ImgPath };
                var queryEditT2 = createQF.Query("documents").AsUpdate(colsT2, dataT2).Where("userId", "@Id");
                SqlResult EditSRT2 = compilerCreate.Compile(queryEditT2);
                string sqlEditT2 = EditSRT2.Sql;
                List<object> bindingsT2 = EditSRT2.Bindings;

                using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
                {
                    sqlCon.Open();
                    //  Query 2 execution 
                    SqlDataAdapter myAdapterT2 = new SqlDataAdapter(sqlEditT2, sqlCon);
                    SqlCommand sqlCmdT2 = new SqlCommand(sqlEditT2, sqlCon);
                    sqlCmdT2.Parameters.AddWithValue("@p2", userModel.Id);
                    sqlCmdT2.Parameters.AddWithValue("@p0", extentionImg);
                    sqlCmdT2.Parameters.AddWithValue("@p1", userModel.ImgPath);
                    sqlCmdT2.ExecuteNonQuery();
                }
            }

            else if ((DocsFile != null) && (ImgFile == null))
            {
                foreach (HttpPostedFileBase files in userModel.DocsFile)
                {
                    var compilerCreate = new SqlServerCompiler();
                    var createQF = new QueryFactory(connection, compilerCreate);
                    // Docs edit
                    string fileName = Path.GetFileNameWithoutExtension(files.FileName);
                    string extention = Path.GetExtension(files.FileName);
                    var FnameDocs = userModel.FirstName + userModel.LastName;
                    fileName = fileName + FnameDocs + extention;
                    userModel.DocsPath = "~/App_Data/File/" + fileName;
                    fileName = Path.Combine(Server.MapPath("~/App_Data/File/"), fileName);
                    files.SaveAs(fileName);
                    // Update in table 2 (Query 2)
                    var colsT2 = new[] { "DocsType", "DocsPath" };
                    var dataT2 = new[] { extention, userModel.DocsPath };
                    var queryEditT2 = createQF.Query("documents").AsUpdate(colsT2, dataT2).Where("userId", "@Id");
                    SqlResult EditSRT2 = compilerCreate.Compile(queryEditT2);
                    string sqlEditT2 = EditSRT2.Sql;
                    List<object> bindingsT2 = EditSRT2.Bindings;

                    using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
                    {
                        sqlCon.Open();

                        //  Query 2 execution 
                        SqlDataAdapter myAdapterT2 = new SqlDataAdapter(sqlEditT2, sqlCon);
                        SqlCommand sqlCmdT2 = new SqlCommand(sqlEditT2, sqlCon);
                        sqlCmdT2.Parameters.AddWithValue("@p2", userModel.Id);
                        sqlCmdT2.Parameters.AddWithValue("@p0", extention);
                        sqlCmdT2.Parameters.AddWithValue("@p1", userModel.DocsPath);
                        sqlCmdT2.ExecuteNonQuery();
                    }
                }
            }

            else if ((ImgFile != null) && (DocsFile != null))
            {
                foreach (HttpPostedFileBase files in userModel.DocsFile)
                {
                    var compilerCreate = new SqlServerCompiler();
                    var createQF = new QueryFactory(connection, compilerCreate);
                    // Docs edit
                    string fileName = Path.GetFileNameWithoutExtension(files.FileName);
                    string extention = Path.GetExtension(files.FileName);
                    var FnameDocs = userModel.FirstName + userModel.LastName;
                    fileName = fileName + FnameDocs + extention;
                    userModel.DocsPath = "~/App_Data/File/" + fileName;
                    fileName = Path.Combine(Server.MapPath("~/App_Data/File/"), fileName);
                    files.SaveAs(fileName);
                    // Images upload codes
                    string imgName = Path.GetFileNameWithoutExtension(userModel.ImgFile.FileName);
                    string extentionImg = Path.GetExtension(userModel.ImgFile.FileName);
                    var FnameImg = userModel.FirstName + userModel.LastName;
                    imgName = imgName + FnameImg;
                    userModel.ImgPath = "~/App_Data/Image/" + imgName;
                    imgName = Path.Combine(Server.MapPath("~/App_Data/Image/"), imgName);
                    userModel.ImgFile.SaveAs(imgName);
                    // Update in table 2 (Query 2)
                    var colsT2 = new[] { "DocsType", "DocsPath", "ImgType", "ImgPath" };
                    var dataT2 = new[] { extention, userModel.DocsPath, extentionImg, userModel.ImgPath };
                    var queryEditT2 = createQF.Query("documents").AsUpdate(colsT2, dataT2).Where("userId", "@Id");
                    SqlResult EditSRT2 = compilerCreate.Compile(queryEditT2);
                    string sqlEditT2 = EditSRT2.Sql;
                    List<object> bindingsT2 = EditSRT2.Bindings;

                    using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
                    {
                        sqlCon.Open();
                        //  Query 2 execution 
                        SqlDataAdapter myAdapterT2 = new SqlDataAdapter(sqlEditT2, sqlCon);
                        SqlCommand sqlCmdT2 = new SqlCommand(sqlEditT2, sqlCon);
                        sqlCmdT2.Parameters.AddWithValue("@p4", userModel.Id);
                        sqlCmdT2.Parameters.AddWithValue("@p0", extention);
                        sqlCmdT2.Parameters.AddWithValue("@p1", userModel.DocsPath);
                        sqlCmdT2.Parameters.AddWithValue("@p2", extentionImg);
                        sqlCmdT2.Parameters.AddWithValue("@p3", userModel.ImgPath);
                        sqlCmdT2.ExecuteNonQuery();
                    }
                }
            }

            return RedirectToAction("Index");
        }

        // GET: User/Delete/5
        public ActionResult Delete(int id)
        {
            var compilerDelete = new SqlServerCompiler();
            var DeleteQF = new QueryFactory(connection, compilerDelete);

            var queryDelete = DeleteQF.Query("mvc_tb").Where("id", "@Id").AsDelete();

            SqlResult DeleteSR = compilerDelete.Compile(queryDelete);
            string sqlDelete = DeleteSR.Sql;
            List<object> bindings = DeleteSR.Bindings;

            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                sqlCon.Open();
                SqlCommand sqlCmd = new SqlCommand(sqlDelete, sqlCon);
                sqlCmd.Parameters.AddWithValue("@p0", id);
                sqlCmd.ExecuteNonQuery();
                sqlCon.Close();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        // check firstname method // HttpPost
        public JsonResult IsFirstNameAvailable(string FirstName)
        {
            return Json(!db.mvc_tb.Any(mvc_tb => mvc_tb.FirstName == FirstName), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        // Check Lastname method // HttpPost
        public JsonResult IsLastNameAvailable(string LastName)
        {
            return Json(!db.mvc_tb.Any(mvc_tb => mvc_tb.LastName == LastName), JsonRequestBehavior.AllowGet);
        }
    }
}

