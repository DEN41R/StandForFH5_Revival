using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StandForFH5Revival
{
    public class AutoshowCheats : CheatsUtilities
    {
        public override string CheatName => "Autoshow Cheats";

        public bool AllCarsEnabled { get; set; } = false;
        public bool RareCarsEnabled { get; set; } = false;
        public bool FreeCarsEnabled { get; set; } = true;

        private static Sql SqlFh5 => Cheats.GetClass<Sql>();

        public async Task<bool> AddAllCars()
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                await Query(SqlQueryTemplates.ADD_ALL_CARS_TO_GARAGE);
                return true;
            }, "AddAllCars");
        }

        public async Task<bool> ShowAllCarsInAutoshow()
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                string queryTemplate = (AllCarsEnabled || RareCarsEnabled)
                    ? SqlQueryTemplates.SHOW_ALL_CARS_IN_AUTOSHOW_UNION
                    : SqlQueryTemplates.SHOW_ALL_CARS_IN_AUTOSHOW;

                await Query(queryTemplate);
                return true;
            }, "ShowAllCarsInAutoshow");
        }

        public async Task<bool> ShowOnlyRareCarsInAutoshow()
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                await Query(SqlQueryTemplates.SHOW_RARE_CARS_IN_AUTOSHOW);
                return true;
            }, "ShowOnlyRareCarsInAutoshow");
        }

        public async Task<bool> MakeAllCarsFree()
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                await Query(SqlQueryTemplates.MAKE_ALL_CARS_FREE);
                return true;
            }, "MakeAllCarsFree");
        }

        public async Task<bool> AddCustomCars(string customCarIds)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                if (string.IsNullOrWhiteSpace(customCarIds))
                {
                    throw new ArgumentException("Car IDs cannot be empty");
                }

                var validCarIds = ParseAndValidateCarIds(customCarIds);
                if (validCarIds.Count == 0)
                {
                    throw new ArgumentException("No valid car IDs found");
                }

                var carIdList = string.Join(", ", validCarIds);
                var query = string.Format(SqlQueryTemplates.ADD_CARS_TO_GARAGE, carIdList);
                
                await Query(query);
                return true;
            }, "AddCustomCars");
        }

        public async Task<bool> AddMultipleCarsByIds(List<int> carIds)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                if (carIds == null || carIds.Count == 0)
                {
                    throw new ArgumentException("Car ID list cannot be empty");
                }

                var validCarIds = carIds.Where(IsValidCarId).Distinct().ToList();
                if (validCarIds.Count == 0)
                {
                    throw new ArgumentException("No valid car IDs found");
                }

                const int batchSize = 50;
                for (int i = 0; i < validCarIds.Count; i += batchSize)
                {
                    var batch = validCarIds.Skip(i).Take(batchSize).ToList();
                    var carIdList = string.Join(", ", batch);
                    var query = string.Format(SqlQueryTemplates.ADD_CARS_TO_GARAGE, carIdList);
                    await Query(query);
                }

                return true;
            }, "AddMultipleCarsByIds");
        }

        public async Task<bool> AddCarsBySeries(int seriesNumber)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var carIds = GetSeriesCarIds(seriesNumber);
                if (carIds.Count == 0)
                {
                    throw new ArgumentException($"No cars found for series {seriesNumber}");
                }

                var carIdList = string.Join(", ", carIds);
                string query = (AllCarsEnabled || RareCarsEnabled)
                    ? string.Format(SqlQueryTemplates.ADD_CARS_BY_SERIES_TO_AUTOSHOW_UNION, carIdList)
                    : string.Format(SqlQueryTemplates.ADD_CARS_BY_SERIES_TO_AUTOSHOW, carIdList);

                await Query(query);
                return true;
            }, "AddCarsBySeries");
        }

        private static async Task Query(string command)
        {
            if (!SqlFh5.WereScansSuccessful)
            {
                await SqlFh5.SqlExecAobScan();
            }

            if (SqlFh5.WereScansSuccessful)
            {
                await Task.Run(() => SqlFh5.Query(command));
            }
            else
            {
                throw new InvalidOperationException("SQL system not initialized");
            }
        }

        public static List<int> ParseAndValidateCarIds(string customCarIds)
        {
            var validCarIds = new List<int>();
            if (string.IsNullOrWhiteSpace(customCarIds))
                return validCarIds;

            var carIdStrings = customCarIds
                .Split(new[] { ',', ' ', '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct();

            foreach (var carIdString in carIdStrings)
            {
                if (int.TryParse(carIdString, out var carId) && IsValidCarId(carId))
                {
                    validCarIds.Add(carId);
                }
            }

            return validCarIds;
        }

        public static bool IsValidCarId(int carId)
        {
            return carId > 0 && carId <= 99999 && carId != 3300;
        }

        public static List<int> GetSeriesCarIds(int series)
        {
            switch (series)
            {
                case 0: return new List<int> { 569, 3069 };
                case 1: return new List<int> { 1270, 3006, 3194, 3196 };
                case 2: return new List<int> { 3289, 3250, 2068, 1319, 1601, 3005, 2987, 257 };
                case 3: return new List<int> { 2474, 295, 2105, 2743, 3373, 2235, 291 };
                case 4: return new List<int> { 3366, 3548, 2297, 2173 };
                case 5: return new List<int> { 2699, 3035, 1253, 3622 };
                case 6: return new List<int> { 1451, 3482, 3195, 3087 };
                case 7: return new List<int> { 3595, 1578, 2194, 255, 3367 };
                case 8: return new List<int> { 269, 3667, 2242, 3318, 1598 };
                case 9: return new List<int> { 1583, 281, 1352 };
                case 10: return new List<int> { 3727, 3711, 3712, 3710, 3713, 3714, 3709, 3715, 3708, 2869, 3160, 3150, 3248 };
                case 11: return new List<int> { 3645, 1281, 2584, 1382, 3547 };
                case 12: return new List<int> { 3359, 3172, 3584, 3677, 2874 };
                case 13: return new List<int> { 1171, 1397, 2908, 1181 };
                case 14: return new List<int> { 3552, 3142, 2504, 3116 };
                case 15: return new List<int> { 1267, 2216, 2128, 1572, 3608 };
                case 16: return new List<int> { 3537, 3689, 1264, 1204, 3182 };
                case 17: return new List<int> { 3620, 398, 2469, 1381 };
                case 18: return new List<int> { 2822, 2140, 3665, 1478, 2648, 2745, 3687 };
                case 19: return new List<int> { 3693, 3549, 3603, 3553, 3604, 3605, 3686, 3662, 3692, 3670, 3720, 3719, 3625, 3520 };
                case 20: return new List<int> { 3672, 3698, 3214, 3583 };
                case 21: return new List<int> { 3746, 3747, 3439, 3597, 3590, 3722 };
                case 22: return new List<int> { 3744, 3616, 1294, 1394, 1090, 3743, 3742, 3741, 9007, 3809, 3808 };
                case 23: return new List<int> { 3085, 1278, 3284, 3706, 1005, 3761 };
                case 24: return new List<int> { 1124, 2489, 2740, 1393, 1032, 2038, 1295, 1661, 3594, 3724, 3320, 3239, 3673, 3606, 3753 };
                case 25: return new List<int> { 1493, 3644, 3734, 3737, 3658, 3657, 3763, 3771 };
                case 26: return new List<int> { 3810, 3795, 3736, 3111, 3533, 3760, 3492, 3334 };
                case 100: return new List<int> { 2973, 2010, 2431, 315, 2793, 326, 2184, 2158, 348, 2937, 3174, 2750, 3252, 3407, 2576, 2751, 3405, 2872, 489, 2162, 1007, 1173, 1314, 3250, 2416, 2148, 2647, 3187, 2486, 432, 363, 455 };
                case 101: return new List<int> { 3556, 3561, 3559, 3562, 3564, 2941, 3568, 2948, 2947, 3570, 2964, 2951, 3572, 3573, 3574, 3577 };
                case 102: return new List<int> { 1171, 1397, 3006, 3194, 3645, 3672 };
                default: return new List<int>();
            }
        }
    }
}