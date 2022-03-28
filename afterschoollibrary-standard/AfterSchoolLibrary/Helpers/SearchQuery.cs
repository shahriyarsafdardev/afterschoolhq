using BasicLibrary;

namespace AfterSchoolLibrary.Helpers
{
    public static class SearchQuery
    {
        public static string LoadAccountSearchQuery(string enroll_today, string cost, string starting,
            decimal latitude, decimal longitude, int PageNum, int PageSize = 9, bool searchHostAccount = false)
        {
            var sql = @"
            IF(OBJECT_ID('tempdb..#temp_accounts_1') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_1 END
			IF(OBJECT_ID('tempdb..#temp_accounts_2') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_2 END
			IF(OBJECT_ID('tempdb..#temp_accounts_3') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_3 END
			IF(OBJECT_ID('tempdb..#temp_accounts_4') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_4 END
			IF(OBJECT_ID('tempdb..#temp_accounts_5') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_5 END
			IF(OBJECT_ID('tempdb..#temp_accounts_6') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_6 END";

            sql += "\n";

            sql += $@"
            SELECT *
			INTO #temp_accounts_1
			FROM (
                SELECT 
                    a.Id as AccountId,
                    a.Uid as AccountUid, 
                    a.Name as AccountName,                    
                    a.UniqueName as AccountUniqueName,
                    ap.CostLow, 
					ap.CostHigh,
                    ap.IsOpenDuringCovid,
					ap.IsELearningCenter,
                    ap.IsVirtualProgram,
					ap.Headline,
                    ap.HeaderImage,
                    ap.Logo,
                    ap.LocationId,
                    ap.AgeStart,
					ap.AgeEnd,
					glStart.Abbreviation as GradeLevelStart,
					glEnd.Abbreviation as GradeLevelEnd,
                    {(latitude.IsNotEmpty() && longitude.IsNotEmpty() && latitude != 0 && longitude != 0 ? "geography::Point(@Latitude, @Longitude, 4326).STDistance(geography::Point(loc.Latitude, loc.Longitude, 4326))/1609.34 as TotalDifference," : "0 as TotalDifference,")}
                    CASE WHEN CAST(GETDATE() as date) BETWEEN CAST(rp.StartDateTime as date) AND CAST(rp.EndDateTime as date) THEN 1 ELSE 0 END as IsAccountNowEnrolling
                FROM 
                    Account a ";

            if (searchHostAccount)
                sql += @"
                    INNER JOIN AccountRelationship ar ON a.ID = ar.VisitorAccountId ";


            sql += @"
                    LEFT JOIN Activity act ON act.AccountId = a.ID AND act.IsDeleted <> 1 AND act.IsPublished = 1 AND act.IsInviteOnly <> 1";

            if (starting.IsNotEmpty())
                sql += @"
                    LEFT JOIN ActivitySchedule acs ON act.ActivityScheduleId = acs.ID --this join will be applied only in case of Starts in filter";

            sql += @"
					LEFT JOIN ActivityRegistrationPeriod arp on arp.ActivityId = act.ID and arp.IsActive = 1
				    LEFT JOIN RegistrationPeriod rp on rp.ID = arp.RegistrationPeriodId
                    LEFT JOIN AccountProfile ap ON ap.AccountId = a.ID
                    LEFT JOIN Location loc ON ap.LocationId = loc.ID

                    LEFT JOIN AccountProperty properties 
	                    ON properties.AccountId = a.ID 
	                    AND properties.AccountPropertyTypeId in (
		                    9, -- servicesOffered
		                    10, -- timing
		                    11, -- pricingOptions
		                    12, 13, 14, 15, --Classifications, SoftSkills, AcademicSubjects, TeachingMethods, this cannot be filtered but only searched.
		                    17) -- SpecialNeedsOffering
                   
                    LEFT JOIN GradeLevel glStart on glStart.ID = ap.GradeLevelStart
				    LEFT JOIN GradeLevel glEnd on glEnd.ID = ap.GradeLevelEnd

                    OUTER APPLY STRING_SPLIT(@Grades, ',') s
                WHERE";

            if (searchHostAccount)
                sql += @"
                        ((@AccountId IS NULL) OR ar.HostAccountId = @AccountId) AND";

            sql += @"
                    a.IsDeleted <> 1 
                    AND a.IsTest <> 1
                    AND a.IsActive = 1
                    AND ap.IsAHQApproved = 1                    
                    AND (
						    (@SearchTerm = '""""') OR
						    CONTAINS((ap.Title, ap.Headline, ap.About), @SearchTerm)  OR
                            CONTAINS((properties.Value), @SearchTerm) OR
                            CONTAINS((loc.Address1, loc.Address2, loc.City, loc.State, loc.PostalCode, loc.Name, loc.StreetNumber, loc.StreetName, loc.Neighborhood, loc.County), @SearchTerm)
					    )
                    AND ((@IsELearningCenter IS NULL) OR ap.IsELearningCenter = @IsELearningCenter)
                    AND ((@IsVirtualProgram IS NULL) OR ap.IsVirtualProgram = @IsVirtualProgram)
                    AND ((@IsOpenDuringCovid IS NULL) OR ap.IsOpenDuringCovid = @IsOpenDuringCovid)
                    AND ((@Gender IS NULL) OR ap.GenderRestriction In (SELECT value FROM STRING_SPLIT(@Gender, ',') WHERE RTRIM(value) <> ''))
                    AND ( -- filter AccountProperties
						((@Services IS NULL) OR (properties.AccountPropertyTypeId = 9 AND properties.Value In (SELECT value FROM STRING_SPLIT(@Services, ',') WHERE RTRIM(value) <> '')))
						AND ((@Timing IS NULL) OR (properties.AccountPropertyTypeId = 10 AND properties.Value In (SELECT value FROM STRING_SPLIT(@Timing, ',') WHERE RTRIM(value) <> '')))
						AND ((@PricingOptions IS NULL) OR (properties.AccountPropertyTypeId =  11 AND properties.Value In (SELECT value FROM STRING_SPLIT(@PricingOptions, ',') WHERE RTRIM(value) <> '')))
						AND ((@SpecialNeeds IS NULL) OR (properties.AccountPropertyTypeId = 17 AND properties.Value In (SELECT value FROM STRING_SPLIT(@SpecialNeeds, ',') WHERE RTRIM(value) <> '')))						
					) -- end filter AccountProperties
                    AND ((@Grades IS NULL) OR (s.value) BETWEEN ap.GradeLevelStart AND ap.GradeLevelEnd)
                    AND (
	                        (@Age IS NULL) OR 
	                        (ap.AgeStart IN (SELECT value FROM STRING_SPLIT(@Age, ',') WHERE RTRIM(value) <> '')) OR 
	                        (ap.AgeEnd IN (SELECT value FROM STRING_SPLIT(@Age, ',') WHERE RTRIM(value) <> ''))
                        )               
            ";

            if (cost.IsNotEmpty())
            {
                sql += @"
                    AND ((@Cost IS NULL)";

                if (cost.Contains("cost_free"))
                    sql += @" OR COALESCE(ap.CostLow, 0) = 0";

                if (cost.Contains("cost_1_20"))
                    sql += @" OR (CostLow <= 20 and CostHigh > 0)";

                if (cost.Contains("cost_21_50"))
                    sql += @" OR (CostLow <= 50 and CostHigh > 20)";

                if (cost.Contains("cost_51_100"))
                    sql += @" OR (CostLow <= 100 and CostHigh > 50)";

                if (cost.Contains("cost_101_200"))
                    sql += @" OR (CostLow <= 200 and CostHigh > 100)";

                if (cost.Contains("cost_201_above"))
                    sql += @" OR CostHigh > 200";

                sql += @")";
            }

            if (enroll_today.IsNotEmpty() && enroll_today.Contains("enroll"))
                sql += @"
                    AND CAST(GETDATE() as date) BETWEEN CAST(rp.StartDateTime as date) AND CAST(rp.EndDateTime as date)
					   ";

            if (starting.IsNotEmpty())
            {
                sql += @"
                    AND ((@Starting IS NULL)";

                if (starting.Contains("starting_0_7"))
                    sql +=
                        @" OR acs.StartDate between dateadd(dd, 0, cast(getdate() as date)) AND dateadd(dd, 7, cast(getdate() as date))";

                if (starting.Contains("starting_7_28"))
                    sql +=
                        @" OR acs.StartDate between dateadd(dd, 8, cast(getdate() as date)) AND dateadd(dd, 28, cast(getdate() as date))";

                if (starting.Contains("starting_30_90"))
                    sql +=
                        @" OR acs.StartDate between dateadd(dd, 29, cast(getdate() as date)) AND dateadd(dd, 90, cast(getdate() as date))";

                sql += @")";
            }

            sql += @"
            ) AS fltData";

            if (latitude.IsNotEmpty() && longitude.IsNotEmpty() && latitude != 0 && longitude != 0)
            {
                sql += $@"
            
	            SELECT 
					DISTINCT AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 0 as Priority, TotalDifference
				FROM #temp_accounts_1
				ORDER BY TotalDifference asc
	            OFFSET {PageNum * PageSize} ROWS 
				FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
	            
				DROP TABLE #temp_accounts_1";

                return sql;
            }

            sql += $@"
			if ((Select DISTINCT Count(DISTINCT AccountId) FROM    #temp_accounts_1  Where IsAccountNowEnrolling = 1) > {(PageNum + 1) * PageSize})
			   begin 
				   if((Select DISTINCT Count(DISTINCT AccountId) FROM    #temp_accounts_1  Where IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is not null AND Headline != '')) > 10)
						begin
							SELECT *
							INTO #temp_accounts_2
							FROM (SELECT DISTINCT 
											AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
											IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 1 as Priority, TotalDifference
											From #temp_accounts_1 
									Where IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is not null AND Headline != '')
							
									) as filteredData
								
							SELECT DISTINCT
								AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
								IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, Priority, TotalDifference
							FROM #temp_accounts_2
							ORDER BY Priority asc
                            OFFSET {PageNum * PageSize} ROWS 
		                    FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
					 end                      

				   else if ((Select Count(DISTINCT AccountId) FROM    #temp_accounts_1  Where IsAccountNowEnrolling = 1 AND HeaderImage is not null ) > {(PageNum + 1) * PageSize})
						begin 
						SELECT *
						INTO #temp_accounts_3
						FROM(
							 SELECT DISTINCT 
										AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
										IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 1 as Priority, TotalDifference
										From #temp_accounts_1 
								Where IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is not null AND Headline != '')		
								
							 UNION ALL				
							 
							 SELECT DISTINCT 
										AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
										IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 2 as Priority, TotalDifference
										From #temp_accounts_1 
								Where IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is null or Headline = '')	
								
								) as filteredData

                          	SELECT DISTINCT
								AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
								IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, Priority, TotalDifference
							FROM #temp_accounts_3
							ORDER BY Priority asc
                            OFFSET {PageNum * PageSize} ROWS 
		                    FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
						end
				   else if ((Select  Count(DISTINCT AccountId) FROM    #temp_accounts_1  Where IsAccountNowEnrolling = 1 AND Headline is not null  ) > {(PageNum + 1) * PageSize})
						begin
						 SELECT *
						INTO #temp_accounts_4
						FROM(
							SELECT DISTINCT 
												AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
												IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 1 as Priority, TotalDifference
												From #temp_accounts_1 
							   Where IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is not null AND Headline != '')	
							 UNION ALL		
							 SELECT DISTINCT 
										AccountId, AccountUid, AccountName, AccountUniqueName,  CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
										IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 2 as Priority, TotalDifference
										From #temp_accounts_1 
								Where IsAccountNowEnrolling = 1 AND HeaderImage is null AND (Headline is not null AND Headline != '')								
						   			
																											
						  ) as filteredData

						   SELECT DISTINCT
								AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
								IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, Priority, TotalDifference
							FROM #temp_accounts_4
							ORDER BY Priority asc
                            OFFSET {PageNum * PageSize} ROWS 
		                    FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
						end
				   else
					   begin
							SELECT *
							INTO #temp_accounts_5
							FROM(SELECT DISTINCT 
											AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
											IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 1 as Priority, TotalDifference
											From #temp_accounts_1 
									Where IsAccountNowEnrolling = 1 
							
							
									) as filteredData

							SELECT DISTINCT
								AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
								IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, Priority, TotalDifference
							FROM #temp_accounts_5
							ORDER BY Priority asc
                           OFFSET {PageNum * PageSize} ROWS 
		                   FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
					  end
		     end
			else
			 begin
				SELECT * INTO #temp_accounts_6 FROM
			    (  --accounts with current activities or now enrolling and has headerimage and has headline
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 1 as Priority, TotalDifference
				FROM #temp_accounts_1 
				WHERE 
					IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is not null AND Headline != '')					
				UNION ALL

				-- accounts with now enrolling and has headerimage but not headline
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 2 as Priority, TotalDifference
				FROM #temp_accounts_1 
				WHERE 
					IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is null or Headline = '')
				UNION ALL

				-- accounts  with now enrolling and has no headerimage but headline
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 3 as Priority, TotalDifference
				FROM #temp_accounts_1 
				WHERE 
					IsAccountNowEnrolling = 1 AND HeaderImage is null AND (Headline is not null AND Headline != '')

				UNION ALL

				-- accounts  with now enrolling but without headerimage and headline
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 4 as Priority, TotalDifference
				FROM #temp_accounts_1 
				WHERE 
					IsAccountNowEnrolling = 1 AND HeaderImage is null AND (Headline is null or Headline = '')

				UNION ALL


				-- all other accounts
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, 0 as IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 5 as Priority, TotalDifference
				FROM #temp_accounts_1
				WHERE
					AccountId NOT IN (
						SELECT DISTINCT AccountId FROM #temp_accounts_1 
						WHERE IsAccountNowEnrolling = 1		
					)

			 ) as Filterdata

			 SELECT DISTINCT
							AccountId, AccountUid, AccountName, AccountUniqueName, CostLow, CostHigh, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
							IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, Priority, TotalDifference
						FROM #temp_accounts_6
						ORDER BY Priority asc
               OFFSET {PageNum * PageSize} ROWS 
		       FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
		    end

			DROP TABLE #temp_accounts_1
            IF(OBJECT_ID('tempdb..#temp_accounts_2') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_2 END 
			IF(OBJECT_ID('tempdb..#temp_accounts_3') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_3 END
			IF(OBJECT_ID('tempdb..#temp_accounts_4') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_4 END
			IF(OBJECT_ID('tempdb..#temp_accounts_5') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_5 END
			IF(OBJECT_ID('tempdb..#temp_accounts_6') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_6 END";

            return sql;
        }
    }
}