var dataTable;

function loadDataTable(status) {
   
    dataTable = $('#myTable').DataTable({
        "ajax": {
            url: '/admin/order/getall?status='+status,
            dataSrc: 'data'
        },
        "columns": [
            { "title": "ID", data: 'id' },
            { "title": "Name", data: 'applicationUser.name' },
            { "title": "Phone Number", data: 'applicationUser.phoneNumber' },
            { "title": "Email", data: 'applicationUser.email' },
            { "title": "Status", data: 'orderStatus' },
            { "title": "Total", data: 'orderTotal' },
            {
                "title": "Actions",
                data: 'id',
                render: function (data) {
                    return `
                        <div class="w-100 btn-group gap-2" role="group">
                            <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-1">
                                <i class="bi bi-list px-2"></i>view
                            </a>
                           
                        </div>
                    `;
                }
            }
        ]
    });
}


$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {

        loadDataTable("inprocess");
    } else if (url.includes("completed")) {

        loadDataTable("completed");
    }

    else if (url.includes("pending")) {

        loadDataTable("pending");
    }
    else if (url.includes("approved")) {

        loadDataTable("approved");
    } else {

        loadDataTable("all");
    }


});