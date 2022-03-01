using BasicLibrary;
using System;

namespace AfterSchoolLibrary.Helpers
{
    public static class SearchQuery
    {
        public static string LoadAccountSearchQuery(string cost, string starting, decimal distance, int PageNum, int PageSize = 9, bool searchHostAccount = false)
        {
            var sql = $@"
            IF(OBJECT_ID('tempdb..#temp_accounts_1') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_1 END
			IF(OBJECT_ID('tempdb..#temp_accounts_2') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_2 END
			IF(OBJECT_ID('tempdb..#temp_accounts_3') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_3 END
			IF(OBJECT_ID('tempdb..#temp_accounts_4') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_4 END
			IF(OBJECT_ID('tempdb..#temp_accounts_5') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_5 END
			IF(OBJECT_ID('tempdb..#temp_accounts_6') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_6 END";

            sql += $"\n";

            sql += $@"
            SELECT *
			INTO #temp_accounts_1
			FROM (
                SELECT 
                    a.Id as AccountId,
                    a.Uid as AccountUid, 
                    a.Name as AccountName,                    
                    a.UniqueName as AccountUniqueName,
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
                    {(distance.IsNotEmpty() && distance > 0 ? "ABS(DistanceFromOrigin - @Distance) as TotalDifference," : "")}
                    CASE WHEN CAST(GETDATE() as date) BETWEEN CAST(rp.StartDateTime as date) AND CAST(rp.EndDateTime as date) THEN 1 ELSE 0 END as IsAccountNowEnrolling
                FROM 
                    Account a ";
            if (searchHostAccount)
            {
                sql += $@"
                    INNER JOIN AccountRelationship ar ON a.ID = ar.VisitorAccountId ";
            }
                    
                    
            sql +=  $@"
                    LEFT JOIN Activity act ON act.AccountId = a.ID AND act.IsDeleted <> 1 AND act.IsPublished = 1 AND act.IsInviteOnly <> 1";

            if (starting.IsNotEmpty())
            {
                sql += $@"
                    LEFT JOIN ActivitySchedule acs ON act.ActivityScheduleId = acs.ID --this join will be applied only in case of Starts in filter";
            }

            sql += $@"
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
            {
                sql += $@"
                        ((@AccountId IS NULL) OR ar.HostAccountId = @AccountId) AND";
            }
                   
            sql += $@"
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
                sql += $@"
                    AND ((@Cost IS NULL)";

                if (cost.Contains("cost_free"))
                    sql += $@" OR COALESCE(ap.CostLow, 0) = 0 or COALESCE(ap.CostHigh, 0) = 0"; 

                if (cost.Contains("cost_1_20"))
                    sql += $@" OR CostLow between 1 AND 20 or CostHigh between 1 AND 20";

				if (cost.Contains("cost_21_50"))
					sql += $@" OR CostLow between 21 AND 50 or CostHigh between 21 AND 50";

				if (cost.Contains("cost_51_100"))
					sql += $@" OR CostLow between 51 AND 100 or CostHigh between 51 AND 100";

				if (cost.Contains("cost_101_200"))
					sql += $@" OR CostLow between 101 AND 200 or CostHigh between 101 AND 200";

                if (cost.Contains("cost_201_above"))
                    sql += $@" OR CostLow > 201 or CostHigh > 201";

                sql += $@")";
            }

            if (starting.IsNotEmpty())
            {
                sql += $@"
                    AND ((@Starting IS NULL)";

                if (starting.Contains("starting_0_7"))
                    sql += $@" OR acs.StartDate between dateadd(dd, 0, cast(getdate() as date)) AND dateadd(dd, 7, cast(getdate() as date))";

                if (starting.Contains("starting_7_28"))
                    sql += $@" OR acs.StartDate between dateadd(dd, 8, cast(getdate() as date)) AND dateadd(dd, 28, cast(getdate() as date))";

                if (starting.Contains("starting_30_90"))
                    sql += $@" OR acs.StartDate between dateadd(dd, 29, cast(getdate() as date)) AND dateadd(dd, 90, cast(getdate() as date))";

                sql += $@")";
            }

            sql += $@"
            ) AS fltData";

            sql += $"\n";

            if (distance.IsNotEmpty() && distance > 0)
            {
                sql += $@"
            
            SELECT *
			INTO #temp_accounts_2
			FROM (
				--accounts with distance radius of 5 miles
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, TotalDifference, 1 as Priority
				FROM #temp_accounts_1 
				WHERE 
					TotalDifference <= 5
				
				UNION ALL

				--accounts with distance radius of 5-10 miles
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, TotalDifference, 2 as Priority
				FROM #temp_accounts_1 
				WHERE 
					TotalDifference > 5 and TotalDifference  <= 10

				UNION ALL

				--accounts with distance radius of 10-25 miles
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, TotalDifference, 3 as Priority
				FROM #temp_accounts_1 
				WHERE 
					TotalDifference > 10 and TotalDifference  <= 25

				UNION ALL

				--all other accounts
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, TotalDifference, 4 as Priority
				FROM #temp_accounts_1 
				WHERE 
					TotalDifference > 25
			) as filteredData							

			SELECT 
				AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
				IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, TotalDifference, Priority
			FROM #temp_accounts_2
			ORDER BY Priority asc, TotalDifference asc
            OFFSET {(PageNum * PageSize)} ROWS 
			FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
            
			DROP TABLE #temp_accounts_1
			DROP TABLE #temp_accounts_2";

            }
            else
            {
                sql += $@"
            
                if ((Select DISTINCT Count(DISTINCT AccountId) FROM    #temp_accounts_1  Where IsAccountNowEnrolling = 1) > {((PageNum + 1) * PageSize)})
				   begin 
					   if((Select DISTINCT Count(DISTINCT AccountId) FROM    #temp_accounts_1  Where IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is not null AND Headline != '')) > 10)
							begin
								SELECT *
								INTO #temp_accounts_2
								FROM (SELECT DISTINCT 
												AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
												IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 1 as Priority
												From #temp_accounts_1 
										Where IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is not null AND Headline != '')
								
										) as filteredData
									
								SELECT DISTINCT
									AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
									IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, Priority
								FROM #temp_accounts_2
								ORDER BY Priority asc
                                OFFSET {(PageNum * PageSize)} ROWS 
			                    FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
						 end                      

					   else if ((Select Count(DISTINCT AccountId) FROM    #temp_accounts_1  Where IsAccountNowEnrolling = 1 AND HeaderImage is not null ) > {((PageNum + 1) * PageSize)})
							begin 
							SELECT *
							INTO #temp_accounts_3
							FROM(
								 SELECT DISTINCT 
											AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
											IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 1 as Priority
											From #temp_accounts_1 
									Where IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is not null AND Headline != '')		
									
								 UNION ALL				
								 
								 SELECT DISTINCT 
											AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
											IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 2 as Priority
											From #temp_accounts_1 
									Where IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is null or Headline = '')	
									
									) as filteredData

                          		SELECT DISTINCT
									AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
									IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, Priority
								FROM #temp_accounts_3
								ORDER BY Priority asc
                                OFFSET {(PageNum * PageSize)} ROWS 
			                    FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
							end
					   else if ((Select  Count(DISTINCT AccountId) FROM    #temp_accounts_1  Where IsAccountNowEnrolling = 1 AND Headline is not null  ) > {((PageNum + 1) * PageSize)})
							begin
							 SELECT *
							INTO #temp_accounts_4
							FROM(
								SELECT DISTINCT 
													AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
													IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 1 as Priority
													From #temp_accounts_1 
								   Where IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is not null AND Headline != '')	
								 UNION ALL		
								 SELECT DISTINCT 
											AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
											IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 2 as Priority
											From #temp_accounts_1 
									Where IsAccountNowEnrolling = 1 AND HeaderImage is null AND (Headline is not null AND Headline != '')								
						   				
																												
							  ) as filteredData

							   SELECT DISTINCT
									AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
									IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, Priority
								FROM #temp_accounts_4
								ORDER BY Priority asc
                                OFFSET {(PageNum * PageSize)} ROWS 
			                    FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
							end
					   else
						   begin
								SELECT *
								INTO #temp_accounts_5
								FROM(SELECT DISTINCT 
												AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
												IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 1 as Priority
												From #temp_accounts_1 
										Where IsAccountNowEnrolling = 1 
								
								
										) as filteredData

								SELECT DISTINCT
									AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
									IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, Priority
								FROM #temp_accounts_5
								ORDER BY Priority asc
                               OFFSET {(PageNum * PageSize)} ROWS 
			                   FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
						  end
			     end
				else
				 begin
					SELECT * INTO #temp_accounts_6 FROM
				    (  --accounts with current activities or now enrolling and has headerimage and has headline
					SELECT DISTINCT 
						AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
						IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 1 as Priority
					FROM #temp_accounts_1 
					WHERE 
						IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is not null AND Headline != '')					
					UNION ALL

					-- accounts with now enrolling and has headerimage but not headline
					SELECT DISTINCT 
						AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
						IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 2 as Priority
					FROM #temp_accounts_1 
					WHERE 
						IsAccountNowEnrolling = 1 AND HeaderImage is not null AND (Headline is null or Headline = '')
					UNION ALL

					-- accounts  with now enrolling and has no headerimage but headline
					SELECT DISTINCT 
						AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
						IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 3 as Priority
					FROM #temp_accounts_1 
					WHERE 
						IsAccountNowEnrolling = 1 AND HeaderImage is null AND (Headline is not null AND Headline != '')

					UNION ALL

					-- accounts  with now enrolling but without headerimage and headline
					SELECT DISTINCT 
						AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
						IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 4 as Priority
					FROM #temp_accounts_1 
					WHERE 
						IsAccountNowEnrolling = 1 AND HeaderImage is null AND (Headline is null or Headline = '')

					UNION ALL


					-- all other accounts
					SELECT DISTINCT 
						AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, 0 as IsAccountNowEnrolling, IsELearningCenter, 
						IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 5 as Priority
					FROM #temp_accounts_1
					WHERE
						AccountId NOT IN (
							SELECT DISTINCT AccountId FROM #temp_accounts_1 
							WHERE IsAccountNowEnrolling = 1		
						)

				 ) as Filterdata

				 SELECT DISTINCT
								AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
								IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, Priority
							FROM #temp_accounts_6
							ORDER BY Priority asc
                   OFFSET {(PageNum * PageSize)} ROWS 
			       FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
			    end

           
            
			DROP TABLE #temp_accounts_1";

            sql += $@"
            IF(OBJECT_ID('tempdb..#temp_accounts_2') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_2 END 
			IF(OBJECT_ID('tempdb..#temp_accounts_3') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_3 END
			IF(OBJECT_ID('tempdb..#temp_accounts_4') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_4 END
			IF(OBJECT_ID('tempdb..#temp_accounts_5') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_5 END
			IF(OBJECT_ID('tempdb..#temp_accounts_6') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_6 END";
        
            }

            return sql;
        }

        /// <summary>
        /// This query can be used to test parameters being sent by command parameters, only for development and debugging purpose
        /// </summary>
        public static string LoadAccountSearchQueryRow(int hostAccountId, string search, string neighborhoods, string grades, string ages, string gender,
            string services, string timing, string pricing, string cost, string starting, string other, string specialNeeds, decimal distance, int PageNum, int PageSize)
        {
            //.. copy this code in Account.SearchVisitorAccountsByHostAccount and check the query
            //var dq = DatabaseQuery;
            //dq.Connection = dc;
            //var sql = Helpers.SearchQuery.LoadAccountSearchQueryRow(hostAccountId, search, neighborhoods, grades, ages, gender, services, timing, pricing, cost, starting, other, specialNeeds, distance, PageNum, PageSize);
            //return dq.ExecuteSelect(sql, msgs);

            var searchTerms = $"\"\"";

            if (search.IsNotBlank())
                searchTerms = $"\"*{search}*\"";

            var sql = $@"
            DECLARE @AccountId NVARCHAR(50),
                @SearchTerm NVARCHAR(250),
                @Neighborhood NVARCHAR(100),
                @Grades NVARCHAR(100),
                @Age NVARCHAR(100),
                @Gender varchar(10),                        
                @Cost varchar(1),
                @Starting varchar(1),
                @IsELearningCenter BIT = NULL,  
                @IsVirtualProgram BIT = NULL, 
                @IsOpenDuringCovid BIT = NULL, 
                @Services NVARCHAR(100),
                @Timing NVARCHAR(100),
                @PricingOptions NVARCHAR(100),
                @SpecialNeeds NVARCHAR(100)";

            if (distance.IsNotEmpty() && distance > 0)
                sql += $@"{Environment.NewLine}
            DECLARE @Distance decimal(10,2)";

            sql += $@"{Environment.NewLine}
                SET @SearchTerm = '{searchTerms}'";

            if (hostAccountId <= 0)
                sql += $@"
                SET @AccountId = NULL ";
            else
                sql += $@"
                SET @AccountId = '{hostAccountId}'";

            if (neighborhoods.IsNotEmpty())
                sql += $@"
                SET @Neighborhood = '{neighborhoods.Replace("'", "''")}'";

            if (grades.IsNotEmpty())
                sql += $@"
                SET @Grades = '{grades}'";

            if (ages.IsNotEmpty())
                sql += $@"
                SET @Age = '{ages}'";

            if (gender.IsNotEmpty())
                sql += $@"
                SET @Gender = '{gender}'";

            if (cost.IsNotEmpty())
                sql += $@"
                SET @Cost = '1'";

            if (starting.IsNotEmpty())
                sql += $@"
                SET @Starting = '1'";

            if (other.IsNotBlank() && other.Contains("e_learning"))
                sql += $@"
                SET @IsELearningCenter = 1";

            if (other.IsNotBlank() && other.Contains("virtual_programming"))
                sql += $@"
                SET @IsVirtualProgram = 1";

            if (other.IsNotBlank() && other.Contains("covid_protections"))
                sql += $@"
                SET @IsOpenDuringCovid = 1";

            if (services.IsNotEmpty())
                sql += $@"
                SET @Services = '{services}'";

            if (timing.IsNotEmpty())
                sql += $@"
                SET @Timing = '{timing}'";

            if (pricing.IsNotEmpty())
                sql += $@"
                SET @PricingOptions = '{pricing}'";

            if (specialNeeds.IsNotEmpty())
                sql += $@"
                SET @SpecialNeeds = '{specialNeeds}'";

            if (distance.IsNotEmpty() && distance > 0)
                sql += $@"
                SET @Distance = {distance}";

            sql += $"\n";

            sql += $@"
            IF(OBJECT_ID('tempdb..#temp_accounts_1') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_1 END
			IF(OBJECT_ID('tempdb..#temp_accounts_2') IS NOT NULL) BEGIN DROP TABLE #temp_accounts_2 END";

            sql += $"\n";

            sql += $@"
            SELECT *
			INTO #temp_accounts_1
			FROM (
                SELECT 
                    a.Id as AccountId,
                    a.Uid as AccountUid, 
                    a.Name as AccountName,                    
                    a.UniqueName as AccountUniqueName,
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
                    {(distance.IsNotEmpty() && distance > 0 ? "ABS(DistanceFromOrigin - @Distance) as TotalDifference," : "")}
                    CASE WHEN CAST(GETDATE() as date) BETWEEN CAST(rp.StartDateTime as date) AND CAST(rp.EndDateTime as date) THEN 1 ELSE 0 END as IsAccountNowEnrolling
                FROM 
                    AccountRelationship ar
                    INNER JOIN Account a ON a.ID = ar.VisitorAccountId
                    LEFT JOIN Activity act ON act.AccountId = ar.VisitorAccountId AND act.IsDeleted <> 1 AND act.IsPublished = 1 AND act.IsInviteOnly <> 1";

            if (starting.IsNotEmpty())
            {
                sql += $@"
                    LEFT JOIN ActivitySchedule acs ON act.ActivityScheduleId = acs.ID --this join will be applied only in case of Starts in filter";
            }

            sql += $@"
					LEFT JOIN ActivityRegistrationPeriod arp on arp.ActivityId = act.ID and arp.IsActive = 1
				    LEFT JOIN RegistrationPeriod rp on rp.ID = arp.RegistrationPeriodId
                    LEFT JOIN AccountProfile ap ON ap.AccountId = ar.VisitorAccountId
                    LEFT JOIN Location loc ON ap.LocationId = loc.ID
                    LEFT JOIN AccountProperty properties on properties.AccountId = ar.VisitorAccountId AND properties.AccountPropertyTypeId in (12, 13, 14, 15)    
                    LEFT JOIN AccountProperty specialNeeds on specialNeeds.AccountId = ar.VisitorAccountId AND specialNeeds.AccountPropertyTypeId = 17
                    LEFT JOIN AccountProperty servicesOffered on servicesOffered.AccountId = ar.VisitorAccountId AND servicesOffered.AccountPropertyTypeId = 9
				    LEFT JOIN AccountProperty timing on timing.AccountId = ar.VisitorAccountId AND timing.AccountPropertyTypeId = 10
                    LEFT JOIN AccountProperty pricingOptions on pricingOptions.AccountId = ar.VisitorAccountId AND pricingOptions.AccountPropertyTypeId = 11
				    LEFT JOIN GradeLevel glStart on glStart.ID = ap.GradeLevelStart
				    LEFT JOIN GradeLevel glEnd on glEnd.ID = ap.GradeLevelEnd
                    OUTER APPLY STRING_SPLIT(@Grades, ',') s
                WHERE 
                    ((@AccountId IS NULL) OR ar.HostAccountId = @AccountId)
                    AND a.IsDeleted <> 1 
                    AND a.IsTest <> 1
                    AND a.IsActive = 1
                    AND ap.IsAHQApproved = 1                    
                    AND (
						    (@SearchTerm = '""""') OR
						    CONTAINS((ap.Title, ap.Headline, ap.About), @SearchTerm)  OR
                            CONTAINS((properties.Value), @SearchTerm) OR
						    CONTAINS((specialNeeds.Value), @SearchTerm) OR
                            CONTAINS((servicesOffered.Value), @SearchTerm) OR
							CONTAINS((timing.Value), @SearchTerm) OR
                            CONTAINS((pricingOptions.Value), @SearchTerm) OR
                            CONTAINS((loc.Address1, loc.Address2, loc.City, loc.State, loc.PostalCode, loc.Name, loc.StreetNumber, loc.StreetName, loc.Neighborhood, loc.County), @SearchTerm)
					    )
                    AND ((@Neighborhood IS NULL) OR loc.Neighborhood IN (SELECT value COLLATE Latin1_General_100_CI_AI FROM STRING_SPLIT(@Neighborhood, ',') WHERE RTRIM(value) <> ''))                    
                    AND ((@IsELearningCenter IS NULL) OR ap.IsELearningCenter = @IsELearningCenter)
                    AND ((@IsVirtualProgram IS NULL) OR ap.IsVirtualProgram = @IsVirtualProgram)
                    AND ((@IsOpenDuringCovid IS NULL) OR ap.IsOpenDuringCovid = @IsOpenDuringCovid)
                    AND ((@Gender IS NULL) OR ap.GenderRestriction In (SELECT value FROM STRING_SPLIT(@Gender, ',') WHERE RTRIM(value) <> ''))
                    AND ((@SpecialNeeds IS NULL) OR specialNeeds.Value In (SELECT value FROM STRING_SPLIT(@SpecialNeeds, ',') WHERE RTRIM(value) <> ''))
                    AND ((@Services IS NULL) OR servicesOffered.Value In (SELECT value FROM STRING_SPLIT(@Services, ',') WHERE RTRIM(value) <> ''))
					AND ((@Timing IS NULL) OR timing.Value In (SELECT value FROM STRING_SPLIT(@Timing, ',') WHERE RTRIM(value) <> ''))
                    AND ((@PricingOptions IS NULL) OR pricingOptions.Value In (SELECT value FROM STRING_SPLIT(@PricingOptions, ',') WHERE RTRIM(value) <> ''))
                    AND ((@Grades IS NULL) OR (s.value) BETWEEN ap.GradeLevelStart AND ap.GradeLevelEnd)
                    AND (
	                        (@Age IS NULL) OR 
	                        (ap.AgeStart IN (SELECT value FROM STRING_SPLIT(@Age, ',') WHERE RTRIM(value) <> '')) OR 
	                        (ap.AgeEnd IN (SELECT value FROM STRING_SPLIT(@Age, ',') WHERE RTRIM(value) <> ''))
                        )               
            ";

            if (cost.IsNotEmpty())
            {
                sql += $@"
                    AND ((@Cost IS NULL)";

                if (cost.Contains("cost_free"))
                    sql += $@" OR COALESCE(act.CostPerRegistrant, 0) = 0";

                if (cost.Contains("cost_0_50"))
                    sql += $@" OR act.CostPerRegistrant between 1 AND 50";

                if (cost.Contains("cost_50_150"))
                    sql += $@" OR act.CostPerRegistrant between 50 AND 150";

                if (cost.Contains("cost_150_above"))
                    sql += $@" OR act.CostPerRegistrant > 150";

                sql += $@")";
            }

            if (starting.IsNotEmpty())
            {
                sql += $@"
                    AND ((@Starting IS NULL)";

                if (starting.Contains("starting_0_7"))
                    sql += $@" OR acs.StartDate between dateadd(dd, 0, cast(getdate() as date)) AND dateadd(dd, 7, cast(getdate() as date))";

                if (starting.Contains("starting_7_28"))
                    sql += $@" OR acs.StartDate between dateadd(dd, 8, cast(getdate() as date)) AND dateadd(dd, 28, cast(getdate() as date))";

                if (starting.Contains("starting_30_90"))
                    sql += $@" OR acs.StartDate between dateadd(dd, 29, cast(getdate() as date)) AND dateadd(dd, 90, cast(getdate() as date))";

                sql += $@")";
            }

            sql += $@"
            ) AS fltData";

            sql += $"\n";

            if (distance.IsNotEmpty() && distance > 0)
            {

                sql += $@"
            
            SELECT *
			INTO #temp_accounts_2
			FROM (
				--accounts with distance radius of 5 miles
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, TotalDifference, 1 as Priority
				FROM #temp_accounts_1 
				WHERE 
					TotalDifference <= 5
				
				UNION ALL

				--accounts with distance radius of 5-10 miles
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, TotalDifference, 2 as Priority
				FROM #temp_accounts_1 
				WHERE 
					TotalDifference > 5 and TotalDifference  <= 10

				UNION ALL

				--accounts with distance radius of 10-25 miles
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, TotalDifference, 3 as Priority
				FROM #temp_accounts_1 
				WHERE 
					TotalDifference > 10 and TotalDifference  <= 25

				UNION ALL

				--all other accounts
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, TotalDifference, 4 as Priority
				FROM #temp_accounts_1 
				WHERE 
					TotalDifference > 25
			) as filteredData							

			SELECT 
				AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
				IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, TotalDifference, Priority
			FROM #temp_accounts_2
			ORDER BY Priority asc, TotalDifference asc
            OFFSET {(PageNum * PageSize)} ROWS 
			FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
            
			DROP TABLE #temp_accounts_1
			DROP TABLE #temp_accounts_2";

            }
            else
            {

                sql += $@"
            
			SELECT *
			INTO #temp_accounts_2
			FROM (
				--accounts with current activities or now enrolling
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 1 as Priority
				FROM #temp_accounts_1 
				WHERE 
					IsAccountNowEnrolling = 1
				
				UNION ALL

				-- accounts with header image or logo next
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 2 as Priority
				FROM #temp_accounts_1 
				WHERE 
					AccountId NOT IN (
						SELECT DISTINCT AccountId FROM #temp_accounts_1 
						WHERE IsAccountNowEnrolling = 1
					) AND (HeaderImage is not null or Logo is not null)

				UNION ALL

				-- all other accounts
				SELECT DISTINCT 
					AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, 0 as IsAccountNowEnrolling, IsELearningCenter, 
					IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, 3 as Priority
				FROM #temp_accounts_1
				WHERE
					AccountId NOT IN (
						SELECT DISTINCT AccountId FROM #temp_accounts_1 
						WHERE IsAccountNowEnrolling = 1		
					)
					AND HeaderImage is null and Logo is null --because we already filtered this in second query, so no need of those accounts now
			) as filteredData							

			SELECT DISTINCT
				AccountId, AccountUid, AccountName, AccountUniqueName, IsOpenDuringCovid, IsAccountNowEnrolling, IsELearningCenter, 
				IsVirtualProgram, Headline, HeaderImage, Logo, LocationId, AgeStart, AgeEnd, GradeLevelStart, GradeLevelEnd, Priority
			FROM #temp_accounts_2
			ORDER BY Priority asc
            OFFSET {(PageNum * PageSize)} ROWS 
			FETCH {(PageNum == 0 ? "first" : "next")} {PageSize} ROWS only;
            
			DROP TABLE #temp_accounts_1
			DROP TABLE #temp_accounts_2";

            }

            return sql;
        }
    }
}
