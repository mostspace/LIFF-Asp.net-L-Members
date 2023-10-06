document.addEventListener("DOMContentLoaded", function (event) {
    let profileSetting = document.getElementById('profile_setting');
    if (profileSetting.value != "") {
        let settingObject = JSON.parse(profileSetting.value);
        console.log(settingObject);
        settingObject.forEach((item) => {
            if (item['data-no'] < 8) {
                let checkbox = document.querySelector(`input[name="form_types_${item['data-no']}"]`);
                if (checkbox) {
                    checkbox.checked = true;
                }

                if (item['required']) {
                    let checkbox = document.querySelector(`input[name="is_requireds[${item['data-no']}]"]`);
                    if (checkbox) {
                        checkbox.checked = true;
                    }
                }
            } else {
                addHtml(item);
            }
        });
    }
    
    
    setTimeout(function () {
        try {
            let alertEle = document.getElementById('alert-show');
            alertEle.classList.add('hidden');
        } catch (e) {
            console.log(e)
        }
    }, 1500)

    let addCard = document.querySelector('.add-card');
    addCard.addEventListener('click', function (e) {
        var cards = document.getElementsByClassName('card');

        // Calculate the length of the cards array
        var number = cards.length + 9;
        let newElement = document.createElement('div');
        newElement.className = 'card my-3';
        newElement.innerHTML =
            `<div class="card-body">
                        <div class="row">
                            <div class="col-md-8">
                                <input class="form-control" name="questions_${number}" placeholder="質問文">
                            </div>
                            <div class="col-md-4">
                                <select class="form-select answer-type" name="answer_types_${number}">
                                    <option value="1" selected>選択肢単独</option>
                                    <option value="2">選択肢複数</option>
                                    <option value="3">テキスト</option>
                                    <option value="4">段落</option>
                                    <option value="5">日付</option>
                                </select>
                            </div>
                            <div class="col-md-12 mt-2">
                                <input class="form-control" name="placeholder_${number}" placeholder="プレイスホルダー">
                            </div>
                            <div class="col-md-12 mt-2">
                                <input class="form-control" name="hint_${number}" placeholder="ヒント">
                            </div>
                        </div>
                        <div class="form-group mt-4 d-flex justify-content-start">
                            <input type="text" class="form-control w-75 options-${number}" name="options_${number}_1" value="選択肢1">
                            <button type="button" class="mx-2 rounded-pill icon-px add-option z-1 btn btn-primary" id="${number}">
                                <i class="mdi mdi-24px mdi-plus px-1"></i>
                            </button>
                        </div>
                    </div>
                    <div class="card-footer py-2">
                        <div class="row">
                            <div class="col-6 mt-2">
                                <div class="custom-control custom-switch">
                                    <input type="checkbox" name="is_required[${number}]" value="1" data-switch="primary" id="customSwitch${number}">
                                    <label for="customSwitch${number}" data-on-label="必須" data-off-label=""></label>
                                </div>
                            </div>
                            <div class="col-6 text-right">
                                <button type="button" class="btn btn-light remove-card"><i class="mdi mdi-24px mdi-delete"></i></button>
                            </div>
                        </div>
                    </div>`;

        var clickedElement = document.querySelector('.add-card');

        // Get the parent node of the clicked element
        var parentElement = clickedElement.parentNode;

        // Insert the new element before the clicked element
        if (parentElement) {
            parentElement.insertBefore(newElement, clickedElement);
        }
    });

    let addOption = document.querySelectorAll('add-option');

    document.addEventListener('change', function (event) {
        var target = event.target;

        if (target.classList.contains('answer-type')) {
            if (target.value == "3" || target.value == "4" || target.value == "5") {
                console.log(target.closest(".card-body"));
                hideFormGroups(target.closest(".card-body"));
            } else {
                showFormGroups(target.closest(".card-body"));
            }
        }
    });

    function hideFormGroups(parentElement) {
        var formGroups = parentElement.getElementsByClassName("form-group");
        for (var i = 0; i < formGroups.length; i++) {
            formGroups[i].classList.add("hidden");
        }
    }

    function showFormGroups(parentElement) {
        var formGroups = parentElement.getElementsByClassName("form-group");
        for (var i = 0; i < formGroups.length; i++) {
            formGroups[i].classList.remove("hidden");
        }
    }

    document.addEventListener('click', function (event) {
        var target = event.target;
        if (target.classList.contains('add-option') || target.parentElement.classList.contains('add-option')) {
            event.preventDefault();
            let number = target.id;
            var options = document.getElementsByClassName(`options-${number}`);

            var cnt = options.length + 1;
            target.closest(".card-body").insertAdjacentHTML('beforeend',
                `<div class="form-group mt-4 d-flex justify-content-start">
                    <input type="text" class="form-control w-75 options-${number}" name="options_${number}_${cnt}" value="">
                    <button type="button" class="mx-2 rounded-pill icon-px remove-option z-1 btn btn-light">
                        <i class="mdi mdi-24px mdi-close-thick px-1"></i>
                    </button>
                    <button type="button" class="mx-2 rounded-pill icon-px add-option z-1 btn btn-primary" id="${number}">
                        <i class="mdi mdi-24px mdi-plus px-1"></i>
                    </button>
                </div>`
            );
        }

        if (target.classList.contains('remove-option') || target.parentElement.classList.contains('remove-option')) {
            event.preventDefault();
            target.closest('.form-group').remove();
        }
        if (target.classList.contains('remove-card') || target.parentElement.classList.contains('remove-card')) {
            target.closest('.card').remove();
        }
    });

    let saveBtn = document.getElementById('saveBtn');
    saveBtn.addEventListener('click', (e) => {
        let form = document.getElementById('saveForm');
        let formData = new FormData(form);
        let jsonData = [];
        for (var [name, value] of formData.entries()) {
            if (name.search("questions_") !== -1) {
                let ordinal = name.split("_")[1];
                let question = {};
                question['label'] = value;
                question['data-type'] = formData.get(`answer_types_${ordinal}`);
                question['data-no'] = ordinal;
                question['ordinal'] = ordinal;
                question['required'] = formData.get(`is_required[${ordinal}]`) == 1 ? true : false;
                question['visivility'] = true;
                question['placeholder'] = formData.get(`placeholder_${ordinal}`);
                question['hint'] = formData.get(`hint_${ordinal}`);
                let dataType = 'text';
                let dataTypeIndex = formData.get(`answer_types_${ordinal}`);
                switch (dataTypeIndex) {
                    case "1":
                        dataType = 'radio';
                        break;
                    case "2":
                        dataType = 'check';
                        break;
                    case "3":
                        dataType = 'text';
                        break;
                    case "4":
                        dataType = 'textarea';
                        break;
                    case "5":
                        dataType = 'date';
                        break;
                    default:
                        dataType = 'radio';
                        break;
                }

                question['data-type'] = dataType;
                if (dataTypeIndex == 3 || dataTypeIndex == 4 || dataTypeIndex == 5) {
                    question['select-item'] = '';
                } else {
                    let options = document.getElementsByClassName(`options-${ordinal}`);
                    let optionsValue = [];
                    Array.from(options).forEach(option => {
                        optionsValue.push(option.value);
                    });
                    question['select-item'] = optionsValue.join(',');
                }
                jsonData.push(question);
            }
            if (name.search("form_types_") !== -1) {
                let ordinal = name.split("_")[2];
                let question = {};

                question['data-no'] = ordinal;
                question['ordinal'] = ordinal;
                question['required'] = formData.get(`is_requireds[${ordinal}]`) == 1 ? true : false;
                question['visivility'] = true;
                question['placeholder'] = '';
                question['hint'] = '';
                question['data-type'] = 'text';

                switch (ordinal) {
                    case 1:
                        question['label'] = '名前';
                        break;
                    case 2:
                        question['label'] = '名前（フリガナ）';
                        break;
                    case 3:
                        question['label'] = '電話番号';
                        break;
                    case 4:
                        question['label'] = 'メールアドレス';
                        break;
                    case 5:
                        question['label'] = '生年月日';
                        question['data-type'] = 'date';
                        break;
                    case 6:
                        question['label'] = '性別';
                        break;
                    case 7:
                        question['label'] = '住所';
                        break;
                    default:
                        break;
                }

                jsonData.push(question);
            }
        }

        let jsonString = JSON.stringify(jsonData);

        profileSetting.value = jsonString;
        submitLineAccountSetting();
    });
});

function addHtml(item) {
    let number = item['data-no'];
    let newElement = document.createElement('div');
    newElement.className = 'card my-3';
    let radioSelected = "";
    let checkSelected = "";
    let textSelected = "";
    let textareaSelected = "";
    let dateSelected = "";
    switch (item['data-type']) {
        case "radio":
            radioSelected = "selected";
            break;
        case "check":
            checkSelected = "selected";
            break;
        case "text":
            textSelected = "selected";
            break;
        case "textarea":
            textareaSelected = "selected";
            break;
        case "date":
            dateSelected = "selected";
            break;
        default:
            break;
    }

    let selectObject = '';
    if (item['data-type'] == "radio" || item['data-type'] == "check") {
        let selectValue = item['select-item'].split(',');
        selectObject = `
            <div class="form-group mt-4 d-flex justify-content-start">
                <input type="text" class="form-control w-75 options-${number}" name="options_${number}_1" value="${selectValue[0]}">
                <button type="button" class="mx-2 rounded-pill icon-px add-option z-1 btn btn-primary" id="${number}">
                    <i class="mdi mdi-24px mdi-plus px-1"></i>
                </button>
            </div>
        `;

        for (let i = 1; i < selectValue.length; i++) {
            selectObject += `<div class="form-group mt-4 d-flex justify-content-start">
                                <input type="text" class="form-control w-75 options-${number}" name="options_${number}_${i + 1}" value="${selectValue[i]}">
                                <button type="button" class="mx-2 rounded-pill icon-px remove-option z-1 btn btn-light">
                                    <i class="mdi mdi-24px mdi-close-thick px-1"></i>
                                </button>
                                <button type="button" class="mx-2 rounded-pill icon-px add-option z-1 btn btn-primary" id="${number}">
                                    <i class="mdi mdi-24px mdi-plus px-1"></i>
                                </button>
                            </div>`;
        }
    } else {
        selectObject = `
            <div class="form-group mt-4 d-flex justify-content-start hidden">
                <input type="text" class="form-control w-75 options-${number}" name="options_${number}_1" value="選択肢1">
                <button type="button" class="mx-2 rounded-pill icon-px add-option z-1 btn btn-primary" id="${number}">
                    <i class="mdi mdi-24px mdi-plus px-1"></i>
                </button>
            </div>
        `;
    }
    newElement.innerHTML =
        `<div class="card-body">
                <div class="row">
                    <div class="col-md-8">
                        <input class="form-control" name="questions_${number}" placeholder="質問文" value="${item['label']}">
                    </div>
                    <div class="col-md-4">
                        <select class="form-select answer-type" name="answer_types_${number}">
                            <option value="1" ${radioSelected}>選択肢単独</option>
                            <option value="2" ${checkSelected}>選択肢複数</option>
                            <option value="3" ${textSelected}>テキスト</option>
                            <option value="4" ${textareaSelected}>段落</option>
                            <option value="5" ${dateSelected}>日付</option>
                        </select>
                    </div>
                    <div class="col-md-12 mt-2">
                        <input class="form-control" name="placeholder_${number}" placeholder="プレイスホルダー" value="${item['placeholder']}">
                    </div>
                    <div class="col-md-12 mt-2">
                        <input class="form-control" name="hint_${number}" placeholder="ヒント" value="${item['hint']}">
                    </div>
                </div>
                ${selectObject}
            </div>
            <div class="card-footer py-2">
                <div class="row">
                    <div class="col-6 mt-2">
                        <div class="custom-control custom-switch">
                            <input type="checkbox" name="is_required[${number}]" value="1" data-switch="primary" id="customSwitch${number}">
                            <label for="customSwitch${number}" data-on-label="必須" data-off-label=""></label>
                        </div>
                    </div>
                    <div class="col-6 text-right">
                        <button type="button" class="btn btn-light remove-card"><i class="mdi mdi-24px mdi-delete"></i></button>
                    </div>
                </div>
            </div>`;

    var clickedElement = document.querySelector('.add-card');
    // Get the parent node of the clicked element
    var parentElement = clickedElement.parentNode;

    // Insert the new element before the clicked element
    if (parentElement) {
        parentElement.insertBefore(newElement, clickedElement);
    }

    if (item['required']) {
        let checkbox = document.querySelector(`input[name="is_required[${number}]"]`);
        if (checkbox) {
            checkbox.checked = true;
        }
    }
}

function submitLineAccountSetting() {

    var Account = {};
    Account.LineaccountId = basic_info.Account_LineaccountId.value;
    Account.LineaccountCode = basic_info.Account_LineaccountCode.value;
    Account.LineaccountShortcode = basic_info.Account_LineaccountShortcode.value;
    Account.LineaccountCreated = basic_info.Account_LineaccountCreated.value;
    Account.ProfileSetting = basic_info.Account_ProfileSetting.value;
    Account.LineaccountName = basic_info.Account_LineaccountName.value;
    Account.LineaccountEmail = basic_info.Account_LineaccountEmail.value;
    Account.EntryPoint = basic_info.Account_EntryPoint.value;
    Account.StartRank = basic_info.Account_StartRank.value;
    Account.PointExpire = basic_info.Account_PointExpire.value;
    Account.MembersCardColor = basic_info.Account_MembersCardColor.value;
    Account.SmaregiContractId = basic_info.Account_SmaregiContractId.value;
    Account.MembersCardLiffId = basic_info.Account_MembersCardLiffId.value;
    Account.LineChannelId = basic_info.Account_LineChannelId.value;
    Account.LineChannelSecret = basic_info.Account_LineChannelSecret.value;
    Account.LineChannelAccessToken = basic_info.Account_LineChannelAccessToken.value;
    Account.TalkMessage = basic_info.Account_TalkMessage.value;
    Account.IsSmaregi = basic_info.switch3.value;
    Account.Istalk = basic_info.switch0.value;
    Account.IsProfile = basic_info.switch1.value;
    Account.MembersCardIsUseCamera = basic_info.switch2.value;
    cardDesign = basic_info.cardDesign.files[0];
    logoImg = basic_info.logoImg.files[0];
    const formData = new FormData();
    formData.append("LineaccountId", Account.LineaccountId)
    formData.append("LineaccountCode", Account.LineaccountCode)
    formData.append("LineaccountShortcode", Account.LineaccountShortcode)
    formData.append("LineaccountCreated", Account.LineaccountCreated)
    formData.append("ProfileSetting", Account.ProfileSetting)
    formData.append("LineaccountName", Account.LineaccountName)
    formData.append("LineaccountEmail", Account.LineaccountEmail)
    formData.append("EntryPoint", Account.EntryPoint)
    formData.append("StartRank", Account.StartRank)
    formData.append("PointExpire", Account.PointExpire)
    formData.append("MembersCardColor", Account.MembersCardColor)
    formData.append("SmaregiContractId", Account.SmaregiContractId)
    formData.append("MembersCardLiffId", Account.MembersCardLiffId)
    formData.append("LineChannelId", Account.LineChannelId)
    formData.append("LineChannelSecret", Account.LineChannelSecret)
    formData.append("LineChannelAccessToken", Account.LineChannelAccessToken)
    formData.append("TalkMessage", Account.TalkMessage)
    formData.append("IsSmaregi", Account.IsSmaregi)
    formData.append("Istalk", Account.Istalk)
    formData.append("IsProfile", Account.IsProfile)
    formData.append("MembersCardIsUseCamera", Account.MembersCardIsUseCamera)
    if (cardDesign) {
        formData.append('cardDesign', cardDesign);
    }
    if (logoImg) {
        formData.append('logoImg', logoImg);
    }
    console.log(formData)
    url = "https://localhost:7116/api/LineAccounts";
    try {
        fetch(url, {
            method: 'POST',
            body: formData
        }).then(res => {
            console.log(res)
            if (res.ok) {
                location.reload();
                console.log('successfully.');
            } else {
                console.error('failed.');
            }
        });
    } catch (error) {
        console.error('An error occurred:', error);
    }
}



$(document).ready(() => {
    $(".data-table").DataTable({
        bPaginate: false,
        bFilter: false,
        bInfo: false,
        bSortable: true,
        bRetrieve: true
    })

    $("#basic_info_submit").click(async function () {
        submitLineAccountSetting();
    });
})