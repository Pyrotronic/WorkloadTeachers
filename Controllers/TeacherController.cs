using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RBPv1._1;
using RBPv1._1.Model;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using TableStyle = DocumentFormat.OpenXml.Wordprocessing.TableStyle;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace RBPWEB.Controllers
{
    public class TeacherController : Controller
    {
        RBPdbContext db;
        public TeacherController(RBPdbContext context)
        {
            db = context;
        }
        public async Task<IActionResult> Index(string searchQuery)
        {
            ViewData["SearchQuery"] = searchQuery;
            var teachers = string.IsNullOrEmpty(searchQuery)
                ? db.Teachers
                : db.Teachers.Where(t => EF.Functions.Like(t.LastName, $"%{searchQuery}%"));
            return View(await teachers.ToListAsync());
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Teachers teachers)
        {
            db.Teachers.Add(teachers);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                Teachers? teachers = await db.Teachers.FindAsync(id);
                if (teachers != null) return View(teachers);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Teachers teachers)
        {
            db.Teachers.Update(teachers);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var teachers = await db.Teachers.FindAsync(id);
            if (id != null)
            {
                db.Teachers.Remove(teachers);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return NotFound();
        }
        public async Task<IActionResult> Delete()
        {
            return View(await db.Teachers.ToListAsync());
        }
        public IActionResult GenerateReportsWord(int? teacherId)
        {
            var teacher = db.Teachers.FirstOrDefault(t => t.TeacherID == teacherId);
            if (teacher == null)
            {
                return NotFound("Преподаватель не найден.");
            }

            string templatePath = "C:/Users/Pyrotronic/source/repos/RBPWEB/RBPWEB/Templates/табельнагрузкипреподавателя.docx";
            string outputPath = $"C:/Users/Pyrotronic/source/repos/RBPWEB/RBPWEB/Reports/teacher_report_{teacher.TeacherID}.docx";

            
            System.IO.File.Copy(templatePath, outputPath, true);

            using (var wordDoc = WordprocessingDocument.Open(outputPath, true))
            {
                var mainPart = wordDoc.MainDocumentPart;
                var body = mainPart.Document.Body;

                
                ReplacePlaceholder(body, "<FIO>", $"{teacher.LastName} {teacher.FirstName.Substring(0, 1)}. {teacher.MiddleName.Substring(0, 1)}.");

               
                var workloads = db.Workloads
                    .Where(w => w.TeacherID == teacherId)
                    .Join(db.Groups, w => w.GroupID, g => g.GroupId, (w, g) => new
                    {
                        w.Subject,
                        g.GroupName,
                        w.Hours,
                        w.ClassType,
                        w.Payment
                    }).ToList();

                
                FillTableWithWorkloads(body, workloads);

                wordDoc.Save();
            }

            var fileBytes = System.IO.File.ReadAllBytes(outputPath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"teacher_report_{teacher.TeacherID}.docx");
        }



        private void ReplacePlaceholder(Body body, string placeholder, string replacement)
        {
            foreach (var paragraph in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            {
                
                var textElements = paragraph.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().ToList();
                string fullText = string.Join("", textElements.Select(te => te.Text));

               
                if (fullText.Contains(placeholder))
                {
                    
                    fullText = fullText.Replace(placeholder, replacement);

                    
                    foreach (var textElement in textElements)
                    {
                        textElement.Remove();
                    }

                    
                    var newRun = new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(fullText));
                    paragraph.AppendChild(newRun);
                }
            }
        }

        private void FillTableWithWorkloads(Body body, IEnumerable<dynamic> workloads)
        {
            
            var table = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Table>().FirstOrDefault();
            if (table == null)
            {
                throw new Exception("Таблица с метками не найдена.");
            }

            
            var templateRow = table.Elements<TableRow>().Last();

            foreach (var workload in workloads)
            {
                var newRow = (TableRow)templateRow.CloneNode(true);

                ReplacePlaceholderInRow(newRow, "<Subject>", workload.Subject);
                ReplacePlaceholderInRow(newRow, "<GroupName>", workload.GroupName);
                ReplacePlaceholderInRow(newRow, "<Hours>", workload.Hours.ToString());
                ReplacePlaceholderInRow(newRow, "<ClassType>", workload.ClassType);
                ReplacePlaceholderInRow(newRow, "<Payment>", workload.Payment.ToString("C"));

                table.AppendChild(newRow);
            }

            
            templateRow.Remove();
        }


        private void ReplacePlaceholderInRow(TableRow row, string placeholder, string replacement)
        {
            foreach (var cell in row.Descendants<TableCell>())
            {
                
                var textElements = cell.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().ToList();
                string fullText = string.Join("", textElements.Select(te => te.Text));

                
                if (fullText.Contains(placeholder))
                {
                    
                    fullText = fullText.Replace(placeholder, replacement);

                    
                    cell.RemoveAllChildren();

                    
                    var run = new Run(new Text(fullText));

                    
                    var paragraphProperties = new ParagraphProperties(
                        new Indentation { Left = "0", Right = "0" }, 
                        new Justification { Val = JustificationValues.Left } 
                    );

                    var paragraph = new Paragraph(paragraphProperties, run);

                    
                    cell.Append(paragraph);
                }
            }
        }
        public IActionResult Documents(int teacherId)
        {
            var teacher = db.Teachers.FirstOrDefault(t => t.TeacherID == teacherId);
            if (teacher == null)
            {
                return NotFound("Преподаватель не найден.");
            }

            return View(teacher);
        }
        public IActionResult GenerateSimpleReportsWord(int teacherId)
        {
            var teacher = db.Teachers.FirstOrDefault(t => t.TeacherID == teacherId);
            if (teacher == null)
            {
                return NotFound("Преподаватель не найден.");
            }

            
            var workloads = db.Workloads
                .Where(w => w.TeacherID == teacherId)
                .ToList();

            if (!workloads.Any())
            {
                return NotFound("У преподавателя нет данных о нагрузке.");
            }

            
            int totalHours = workloads.Sum(w => w.Hours);
            decimal totalPayment = workloads.Sum(w => w.Payment);

            string templatePath = "C:/Users/Pyrotronic/source/repos/RBPWEB/RBPWEB/Templates/ОтчётобОплатеПреподавателя.docx";
            string outputPath = $"C:/Users/Pyrotronic/source/repos/RBPWEB/RBPWEB/Reports/teacher_simplereport_{teacher.TeacherID}.docx";

            
            System.IO.File.Copy(templatePath, outputPath, true);

           
            using (var wordDoc = WordprocessingDocument.Open(outputPath, true))
            {
                var mainPart = wordDoc.MainDocumentPart;
                var body = mainPart.Document.Body;

               
                ReplacePlaceholder(body, "<FIO>", $"{teacher.LastName} {teacher.FirstName.Substring(0, 1)}. {teacher.MiddleName.Substring(0, 1)}.");
                ReplacePlaceholder(body, "<Hours>", totalHours.ToString());
                ReplacePlaceholder(body, "<SumPayment>", totalPayment.ToString("C"));

                
                wordDoc.Save();
            }

            
            var fileBytes = System.IO.File.ReadAllBytes(outputPath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"teacher_simplereport_{teacher.TeacherID}.docx");
        }

        public async Task<IActionResult> GenerateExcelReport(int? teacherId)
        {
            var teacher = db.Teachers.FirstOrDefault(t => t.TeacherID == teacherId);
            if (teacher == null)
            {
                throw new Exception("Преподаватель не найден.");
            }

            
            string templatePath = "C:/Users/Pyrotronic/source/repos/RBPWEB/RBPWEB/Templates/НагрузкаПреподавателя.xlsx";
            
            string outputPath = $"C:/Users/Pyrotronic/source/repos/RBPWEB/RBPWEB/Reports/НагрузкаПреподавателя_{teacher.TeacherID}.xlsx";

            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

              
                worksheet.Cells["E1"].Value = DateTime.Now.ToShortDateString(); 
                worksheet.Cells["C3"].Value = $"{teacher.LastName} {teacher.FirstName.Substring(0, 1)}. {teacher.MiddleName.Substring(0, 1)}.";

                
                var workloads = db.Workloads
                    .Where(w => w.TeacherID == teacherId)
                    .Join(db.Groups, w => w.GroupID, g => g.GroupId, (w, g) => new
                    {
                        w.Subject,
                        g.GroupName,
                        w.Hours,
                        w.ClassType,
                        w.Payment
                    }).ToList();

                
                int startRow = 5; 
                foreach (var workload in workloads)
                {
                    worksheet.Cells[startRow, 1].Value = workload.Subject;
                    worksheet.Cells[startRow, 2].Value = workload.GroupName;
                    worksheet.Cells[startRow, 3].Value = workload.Hours;
                    worksheet.Cells[startRow, 4].Value = workload.ClassType;
                    worksheet.Cells[startRow, 5].Value = workload.Payment;

                    startRow++;
                }

                
                package.SaveAs(new FileInfo(outputPath));
            }

            
            var fileBytes = System.IO.File.ReadAllBytes(outputPath);
            var fileName = $"НагрузкаПреподавателя_{teacher.TeacherID}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
