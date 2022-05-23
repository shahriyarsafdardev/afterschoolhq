select id, Text
from FormQuestionAnswer
where Text like ',%'

begin tran

select count(1) from FormQuestionAnswer
where Text like ',%'

update FormQuestionAnswer
set Text = TRIM(',' from Text )
where Text like ',%'

select id, Text
from FormQuestionAnswer
where Text like ',%'

select count(1) from FormQuestionAnswer
where Text like ',%'

--select id, Text
--from FormQuestionAnswer
--where id = 1239571

commit