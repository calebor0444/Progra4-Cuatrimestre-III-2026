using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace TiendaFluent.Data;

// Traduce los errores de SQL Server que provocan las restricciones definidas con Fluent API
public static class ErroresBd
{
    public static bool EsDuplicado(DbUpdateException ex) =>
        ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627);

    public static bool EsLlaveForanea(DbUpdateException ex) =>
        ex.InnerException is SqlException sql && sql.Number == 547 && !sql.Message.Contains("CHECK");

    public static bool EsCheck(DbUpdateException ex) =>
        ex.InnerException is SqlException sql && sql.Number == 547 && sql.Message.Contains("CHECK");
}
