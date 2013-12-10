
select [Id], [ImgUrl], [Title], [TopicType], [Description], [LastModified], [CreatedOn]
from (
	select [Topic].[Id], [Topic].[ImgUrl], [Topic].[Title], [Topic].[TopicType], [Topic].[Description], [Topic].[LastModified], [Topic].[CreatedOn],
		   (select round(
					cast((log(case when abs([positive] - [negative]) > 1 then abs([positive] - [negative]) else 1 end) 
						* sign([positive] - [negative])) + (datediff(second, '20130101', [Topic].[CreatedOn]) / 45000)
					as numeric), 7) 
				as [hot_score]
			from (
				select 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 1 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [positive], 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 0 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [negative]
				) tmpCount
			where [positive] + [negative] > 0) as [score]
	from [Topic]
	order by score desc
	offset 0 rows fetch next 12 rows only
) tmpScoreSort

