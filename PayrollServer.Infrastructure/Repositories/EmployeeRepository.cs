using Microsoft.EntityFrameworkCore;
using PayrollServer.Domain.Entities;
using PayrollServer.Domain.Exceptions;
using PayrollServer.Domain.Interfaces.Repositories;
using PayrollServer.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayrollServer.Infrastructure.Repositories
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Employee>> GetEmployeesWithDetailsAsync(int page, int pageSize, string? searchTerm = null, int? departmentId = null, int? jobGradeId = null)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobGrade)
                .Include(e => e.SalaryRecords)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(e => 
                    e.FirstName.ToLower().Contains(searchTerm) || 
                    e.LastName.ToLower().Contains(searchTerm) || 
                    e.EmployeeNumber.ToLower().Contains(searchTerm) || 
                    e.Email.ToLower().Contains(searchTerm));
            }

            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            if (jobGradeId.HasValue)
            {
                query = query.Where(e => e.JobGradeId == jobGradeId.Value);
            }

            // Apply pagination
            return await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Employee> GetEmployeeWithDetailsAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobGrade)
                .Include(e => e.SalaryRecords.OrderByDescending(s => s.EffectiveDate))
                .Include(e => e.AbsenceRecords.OrderByDescending(a => a.Year).ThenByDescending(a => a.Month))
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                throw new EntityNotFoundException("Employee", id);
            }

            return employee;
        }

        public async Task<IEnumerable<Employee>> GetAllWithDetailsAsync()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobGrade)
                .Include(e => e.SalaryRecords.OrderByDescending(s => s.EffectiveDate))
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
        }

        public async Task<Employee> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobGrade)
                .Include(e => e.SalaryRecords.OrderByDescending(s => s.EffectiveDate))
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId)
        {
            return await _context.Employees
                .Include(e => e.JobGrade)
                .Where(e => e.DepartmentId == departmentId)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetByJobGradeAsync(int jobGradeId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.JobGradeId == jobGradeId)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetAllActiveEmployeesAsync()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobGrade)
                .Where(e => !e.IsDeleted && e.Status == "Active")
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
        }

        public async Task<Employee> GetEmployeeByEmployeeNumberAsync(string employeeNumber)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobGrade)
                .Include(e => e.SalaryRecords.OrderByDescending(s => s.EffectiveDate))
                .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber);
        }

        public async Task<bool> IsEmployeeNumberUniqueAsync(string employeeNumber, int? excludeId = null)
        {
            var query = _context.Employees.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeId.Value);
            }

            return !await query.AnyAsync(e => e.EmployeeNumber == employeeNumber);
        }

        public async Task<bool> IsDuplicateEmailAsync(string email, int? excludeId = null)
        {
            var query = _context.Employees.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeId.Value);
            }

            return await query.AnyAsync(e => e.Email == email);
        }

        public async Task<Employee> GetDeletedEmployeeByIdAsync(int id)
        {
            return await _context.Employees
                .IgnoreQueryFilters() // Include soft-deleted entities
                .FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted);
        }

        public async Task<int> GetTotalCountAsync(string? searchTerm = null, int? departmentId = null, int? jobGradeId = null)
        {
            var query = _context.Employees.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(e => 
                    e.FirstName.ToLower().Contains(searchTerm) || 
                    e.LastName.ToLower().Contains(searchTerm) || 
                    e.EmployeeNumber.ToLower().Contains(searchTerm) || 
                    e.Email.ToLower().Contains(searchTerm));
            }

            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            if (jobGradeId.HasValue)
            {
                query = query.Where(e => e.JobGradeId == jobGradeId.Value);
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Employee>> GetRecentEmployeesWithDetailsAsync(int count)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobGrade)
                .OrderByDescending(e => e.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetEmployeeCountByDepartmentAsync(int departmentId)
        {
            return await _context.Employees
                .Where(e => e.DepartmentId == departmentId && !e.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetEmployeeCountByJobGradeAsync(int jobGradeId)
        {
            return await _context.Employees
                .Where(e => e.JobGradeId == jobGradeId && !e.IsDeleted)
                .CountAsync();
        }
    }
} 