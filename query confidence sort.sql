---------------------------------------------------------------------------
-- CONFIDENCE SORT TÓPICO
-- para ordenar TOP tópicos de acordo com a votação
-- query baseada em:
-- https://github.com/reddit/reddit/blob/master/r2/r2/lib/db/_sorts.pyx
-- http://www.evanmiller.org/how-not-to-sort-by-average-rating.html
---------------------------------------------------------------------------
select [Id], [ImgUrl], [Title], [TopicType], [Description]
from (
	select top 10 [Topic].[Id], [Topic].[ImgUrl], [Topic].[Title], [Topic].[TopicType], [Topic].[Description],
	 (select ((([positive] + 1.9208) / ([positive] + [negative]) - 
			  1.96 * SQRT(([positive] * [negative]) / ([positive] + [negative]) + 0.9604) / 
			  ([positive] + [negative])) / (1 + 3.8416 / ([positive] + [negative]))) as [confidence_score]
			from (
				select 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 1 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [positive], 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 0 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [negative]
				) tmpCount
			where [positive] + [negative] > 0) as [score]
	from [Topic]
	order 
	by score desc
) tmpScoreSort