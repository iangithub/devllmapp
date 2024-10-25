using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.SemanticKernel;

public class RoomBookingPlugin
{
    [KernelFunction, Description("Get the available rooms information.")]
    public string GetAvailableRooms()
    {
        return JsonSerializer.Serialize(new List<AvailableRoomInfo>()
        {
            new AvailableRoomInfo()
            {
                RoomNo = "101",
                RoomType = "Single",
                Availdate = new DateOnly(2024, 9, 11)
            },
            new AvailableRoomInfo()
            {
                RoomNo = "102",
                RoomType = "Double",
                Availdate = new DateOnly(2024, 9, 19)
            },
            new AvailableRoomInfo()
            {
                RoomNo = "102",
                RoomType = "Double",
                Availdate = new DateOnly(2024, 9, 20)
            },
            new AvailableRoomInfo()
            {
                RoomNo = "102",
                RoomType = "Double",
                Availdate = new DateOnly(2024, 9, 21)
            },
            new AvailableRoomInfo()
            {
                RoomNo = "103",
                RoomType = "Single",
                Availdate = new DateOnly(2024, 9, 19)
            },
             new AvailableRoomInfo()
            {
                RoomNo = "103",
                RoomType = "Single",
                Availdate = new DateOnly(2024, 9, 20)
            },
            new AvailableRoomInfo()
            {
                RoomNo = "104",
                RoomType = "Double",
               Availdate = new DateOnly(2024, 9, 20)
            }
        });
    }

    //參數均為string，對LLM來說everything is string
    [KernelFunction, Description("Get the order information of reserve accommodation.")]
    public string GetOrder(
        [Description("The string of the Room.")]
        string RoomNo,
        [Description("The date of checkin date.")]
        string Sdate,
        [Description("The date of checkout date.")]
        string Edate)
    {
        var orderNo = new Random(Seed: 1).Next(10000, 99999).ToString();
        return $"RoomNo: {RoomNo}, Sdate: {Sdate}, Edate: {Edate}, OrderNo: {orderNo}";
    }
}



public class AvailableRoomInfo
{
    public string RoomNo { get; set; }
    public string RoomType { get; set; }
    public DateOnly Availdate { get; set; }
}
