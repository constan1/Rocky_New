

var dataTable;


$(document).ready(function () {
    loadDataTable("GetInquiryList")
});

function loadDataTable(url) {
    databale = $('#tblData').DataTable({
        "ajax": {
            "url": "/Inquiry/" + url
        },

        "columns": [

            { "data": "id", "width": "10%" },
            { "data": "fullName", "width": "15%" },
            { "data": "phhoneNumber", "width": "15%" },
            { "data": "email", "width": "15%" },

            {
                "data": "id",
                "render": function (data) {
                    return `
                           <div class="text-center">
                            <a href="/Inquiry/Details/${data}" class="btn btn-success text-white" style="cursor:pointer">
                        <i class="fas fa-edit"></li>
                                </a>
                            </div>`;
                },
                "width": "5%"
            }

        ]
    }
    );
}