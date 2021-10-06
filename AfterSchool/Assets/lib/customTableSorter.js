// give class ".sort" to li element outside the table
// indicate column number as "data-index"
// indicate order of sorting "asc" for ascending and "desc" for descending
// add "sortTable" css class to table for enable sorting
$(document).ready(function () {
    $('.sort').click(function () {
        var index = $(this).attr("data-index");
        var order = $(this).attr("data-order");
        $(".selectedFilter").removeClass("selectedFilter");
        $(this).find("a").addClass("selectedFilter");
        var table = $(".sortTable");
        var rows = table.find('tr:gt(0)').toArray().sort(comparer(index))
        this.asc = !this.asc
        if (order == "desc") { rows = rows.reverse(); }
        for (var i = 0; i < rows.length; i++) { table.append(rows[i]) }
    })
    function comparer(index) {
        return function (a, b) {
            var valA = getCellValue(a, index), valB = getCellValue(b, index)
            return $.isNumeric(valA) && $.isNumeric(valB) ? valA - valB : valA.toString().localeCompare(valB)
        }
    }
    function getCellValue(row, index) { return $(row).children('td').eq(index).text() }

})