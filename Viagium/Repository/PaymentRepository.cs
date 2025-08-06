using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Viagium.Models;
using Viagium.Data;
using Viagium.Repository.Interface;

namespace Viagium.Repository;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
    }

    public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
    }

    public async Task<Payment?> GetByAsaasIdAsync(string asaasId)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.PaymentIdAsaas == asaasId);
    }

    public async Task FinalizePaymentAsync(Payment payment)
    {
        // ✅ CORREÇÃO: Apenas atualiza a entidade, não salva diretamente
        _context.Payments.Update(payment);
        // Removido: await _context.SaveChangesAsync(); - isso deve ser feito pelo UnitOfWork
    }


}