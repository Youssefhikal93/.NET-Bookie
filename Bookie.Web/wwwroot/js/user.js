var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/user/getall' },
        dataSrc: 'data',
        "columns": [
            { "title": "Name", "data": "name", "width": "15%" },
            { "title": "Email","data": "email", "width": "15%" },
            { "title": "Phone ", "data": "phoneNumber", "width": "10%" },
            { "title": "Company", "data": "company.name", "width": "10%" },
            { "title": "Role","data": "role", "width": "10%" },
            {
                data:{ id:'id',lockoutEnd:"lockoutEnd"},
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();

                    if (lockout > today) {
                        //means the customer is locked
                        return `
                        <div class="text-center">
                        
                     <a Onclick=lockUnlock('${data.id}') class="btn btn-success text-white mx-2"> 
                     <i class="bi bi-ublock-fill"></i> unlock user
                     </a>      
                     <a href="/Admin/User/RoleMangement?userId=${data.id}" class="btn btn-danger text-white mx-2"> 
                     <i class="bi bi-pencil-square"></i> Permisson
                     </a> 
                           </div>`
                    } else {
                        return `   <div class="text-center">

                            <a Onclick=lockUnlock('${data.id}') class="btn btn-danger text-white mx-2">
                                <i class="bi bi-ublock-fill"></i> lock user
                            </a>
                            <a href="/Admin/User/RoleMangement?userId=${data.id}" class="btn btn-danger text-white mx-2">
                                <i class="bi bi-pencil-square"></i> Permisson
                            </a>
                        </div>`
                    }

                   
                },
                "width": "25%"
            }
        ]
    });
}

function lockUnlock(id) {
    $.ajax({
        type: "POST",
        url: "/Admin/User/LockUnlock",
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message)
                dataTable.ajax.reload();
            }
        }
    })
}