namespace ShopWave
{
    public interface ICouponService
    {
        bool IsValid(string code);
        int  GetDiscount(string code);
        void MarkAsUsed(string code);
    }
}
