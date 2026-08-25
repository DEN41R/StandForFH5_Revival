namespace StandForFH5Revival
{
    public static class SqlQueryTemplates
    {
        public const string ADD_CARS_TO_GARAGE = @"
            INSERT OR IGNORE INTO Profile0_Career_Garage (CarId, Owned) 
            SELECT Id, 1 FROM Data_Car 
            WHERE Id IN ({0}) AND Id != 3300;
            
            INSERT OR IGNORE INTO Profile0_FreeCars (CarId, FreeCount) 
            SELECT Id, 1 FROM Data_Car 
            WHERE Id IN ({0}) AND Id != 3300;
        ";

        public const string ADD_ALL_CARS_TO_GARAGE = @"
            DROP TABLE IF EXISTS AllCarsUnified;
            CREATE TABLE AllCarsUnified AS 
            SELECT DISTINCT Id FROM Data_Car WHERE Id != 3300
            UNION
            SELECT DISTINCT CarId AS Id FROM Profile0_Career_Garage WHERE CarId IS NOT NULL AND CarId != 3300
            UNION
            SELECT DISTINCT ContentId AS Id FROM ContentOffersMapping WHERE ContentId IS NOT NULL AND ContentId != 3300
            UNION
            SELECT DISTINCT CarId AS Id FROM Profile0_FreeCars WHERE CarId IS NOT NULL AND CarId != 3300
            UNION
            SELECT DISTINCT CarId AS Id FROM Data_Car_Buckets WHERE CarId IS NOT NULL AND CarId != 3300
            ORDER BY Id;
            
            INSERT INTO ContentOffersMapping (OfferId, ContentId, ContentType, IsPromo, IsAutoRedeem, ReleaseDateUTC, Quantity) 
            SELECT 99, Id, 1, 0, 1, NULL, 1 FROM AllCarsUnified
            WHERE Id NOT IN (SELECT CarId FROM Profile0_Career_Garage WHERE CarId IS NOT NULL)
            AND Id NOT IN (SELECT ContentId FROM ContentOffersMapping WHERE ContentId IS NOT NULL AND OfferId = 99);
            
            INSERT INTO Profile0_FreeCars 
            SELECT Id, 1 FROM AllCarsUnified
            WHERE Id NOT IN (SELECT CarId FROM Profile0_FreeCars WHERE CarId IS NOT NULL);
            
            UPDATE Profile0_FreeCars SET FreeCount = 1;
        ";

        public const string SHOW_ALL_CARS_IN_AUTOSHOW = @"
            INSERT OR IGNORE INTO ContentOffersMapping (OfferId, ContentId, ContentType, IsPromo, IsAutoRedeem, ReleaseDateUTC, Quantity) 
            SELECT 99, Id, 1, 0, 0, NULL, 1 FROM Data_Car 
            WHERE Id != 3300
            AND Id NOT IN (SELECT CarId FROM Profile0_Career_Garage WHERE CarId IS NOT NULL)
            AND Id NOT IN (SELECT ContentId FROM ContentOffersMapping WHERE ContentId IS NOT NULL AND OfferId = 99);
        ";

        public const string SHOW_ALL_CARS_IN_AUTOSHOW_UNION = @"
            INSERT OR IGNORE INTO ContentOffersMapping (OfferId, ContentId, ContentType, IsPromo, IsAutoRedeem, ReleaseDateUTC, Quantity) 
            SELECT 99, Id, 1, 0, 0, NULL, 1 FROM (SELECT DISTINCT Id FROM AutoshowTable UNION SELECT DISTINCT Id FROM Data_Car) 
            WHERE Id != 3300
            AND Id NOT IN (SELECT CarId FROM Profile0_Career_Garage WHERE CarId IS NOT NULL)
            AND Id NOT IN (SELECT ContentId FROM ContentOffersMapping WHERE ContentId IS NOT NULL AND OfferId = 99);
        ";

        public const string SHOW_RARE_CARS_IN_AUTOSHOW = @"
            INSERT OR IGNORE INTO ContentOffersMapping (OfferId, ContentId, ContentType, IsPromo, IsAutoRedeem, ReleaseDateUTC, Quantity) 
            SELECT 99, Id, 1, 0, 0, NULL, 1 FROM Data_Car 
            WHERE Id != 3300
            AND NotAvailableInAutoshow = 1
            AND Id NOT IN (SELECT CarId FROM Profile0_Career_Garage WHERE CarId IS NOT NULL)
            AND Id NOT IN (SELECT ContentId FROM ContentOffersMapping WHERE ContentId IS NOT NULL AND OfferId = 99);
        ";

        public const string MAKE_ALL_CARS_FREE = @"
            UPDATE ContentOffersMapping 
            SET IsAutoRedeem = 1 
            WHERE ContentType = 1 
            AND OfferId = 99
            AND ContentId IS NOT NULL 
            AND ContentId != 3300;
        ";

        public const string RESTORE_ORIGINAL_CAR_PRICES = @"
            UPDATE ContentOffersMapping 
            SET IsAutoRedeem = 0 
            WHERE ContentType = 1 
            AND OfferId = 99
            AND ContentId IS NOT NULL 
            AND ContentId != 3300
            AND ContentId IN (
                SELECT Id FROM Data_Car 
                WHERE NotAvailableInAutoshow = 0 
                AND Id != 3300
            );
        ";

        public const string ADD_CARS_BY_SERIES_TO_GARAGE = @"
            INSERT OR IGNORE INTO Profile0_Career_Garage (CarId, Owned) 
            SELECT Id, 1 FROM Data_Car 
            WHERE Id IN ({0}) AND Id != 3300;
            
            INSERT OR IGNORE INTO Profile0_FreeCars (CarId, FreeCount) 
            SELECT Id, 1 FROM Data_Car 
            WHERE Id IN ({0}) AND Id != 3300;
        ";

        public const string ADD_CARS_BY_SERIES_TO_AUTOSHOW = @"
            INSERT INTO ContentOffersMapping (OfferId, ContentId, ContentType, IsPromo, IsAutoRedeem, ReleaseDateUTC, Quantity) 
            SELECT 99, Id, 1, 0, 0, NULL, 1 FROM Data_Car 
            WHERE Id IN ({0})
            AND Id NOT IN (SELECT ContentId AS Id FROM ContentOffersMapping WHERE ContentId IS NOT NULL) 
            AND Id != 3300 AND Id NOT IN (SELECT CarId FROM Profile0_Career_Garage);
        ";

        public const string ADD_CARS_BY_SERIES_TO_AUTOSHOW_UNION = @"
            INSERT INTO ContentOffersMapping (OfferId, ContentId, ContentType, IsPromo, IsAutoRedeem, ReleaseDateUTC, Quantity) 
            SELECT 99, Id, 1, 0, 0, NULL, 1 FROM (SELECT Id FROM AutoshowTable UNION SELECT Id FROM Data_Car) 
            WHERE Id IN ({0})
            AND Id NOT IN (SELECT ContentId AS Id FROM ContentOffersMapping WHERE ContentId IS NOT NULL) 
            AND Id != 3300 AND Id NOT IN (SELECT CarId FROM Profile0_Career_Garage);
        ";
    }
}