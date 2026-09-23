const urlParams = new URLSearchParams(window.location.search);
const workType = urlParams.get("type");

// url取得
var baseUrl = window.location.origin;
var pathName = window.location.pathname.split('/');
if (pathName.length > 2)
    baseUrl = baseUrl + "/" + pathName[1];

// -----------------------------------部品補充運搬画面-----------------------------------//
// SignalRを使用して接続を初期化する
const isReplenishment = document.getElementById("replenishment-page");
if (isReplenishment) {
    var connectionReplenishment = new signalR.HubConnectionBuilder().withUrl("replenishmentHub").build();

    $(function () {
        connectionReplenishment.start().then(function () {
            InvokeReplenishment();
        })
    });

    // 短い遅延後に再接続を試みる
    var closeConnectReplenishmentCount = 0;
    connectionReplenishment.onclose(function (error) {
        setTimeout(function () {
            connectionReplenishment.start().then(function () {
                InvokeReplenishment();
            })
            closeConnectReplenishmentCount += 1;
            console.log("Error - onclose 再接続" + closeConnectReplenishmentCount + "回目");
            // 接続が2回以上失われた場合はページをリロード
            if (closeConnectReplenishmentCount >= 2)
                window.location.reload();
        }, 500);
    });

    // ページを離れた時やリロードしたタイミングで接続を止める
    window.addEventListener('unload', function () {
        console.log("addEventListener - unload");
        connectionReplenishment.stop();
    });


    // ハブのメソッドを呼び出す
    function InvokeReplenishment() {
        connectionReplenishment.invoke("SendReplenishments").catch(function (error) {
            console.log("Error - invoke catch");
            $(".connectionReplenishmentError").text(error);
            $(".connectionReplenishmentError").show();
        });
    }


    // エラー発生時
    connectionReplenishment.on("Error", (error) => {
        console.log("Error - on");
        $(".connectionReplenishmentError").text(error);
        $(".connectionReplenishmentError").show();
    });


    // グリッドに依頼をバインドする
    connectionReplenishment.on("ReceivedReplenishments", function (replenishments) {
        BindReplenishmentToGrid(replenishments);
    });
}

function BindReplenishmentToGrid(replenishments) {
    $('#tblReplenishmentLeft tbody').empty();
    $('#tblReplenishmentRight tbody').empty();

    var tableLeftDom = document.getElementById('tblReplenishmentLeft');
    var tableRightDom = document.getElementById('tblReplenishmentRight');

    if (tableLeftDom !== null) {
        var table = tableLeftDom.getElementsByTagName('tbody')[0];
        var table1 = tableRightDom.getElementsByTagName('tbody')[0];

        const renderedTotalButtons = new Set();
        const completedMachines = new Set();

        // 左のテーブル取得
        var replenishments1 = replenishments.slice(0, 6);
        createTable(replenishments1, table);

        // 右のテーブル取得
        var replenishmentsTmp = replenishments.length - replenishments1.length;
        if (replenishmentsTmp > 0) {
            var replenishments2 = replenishments.slice(6, 12);
            createTable(replenishments2, table1);
        }

        // テーブルを作成
        function createTable(replenishments, table) {
            if (replenishments.length > 0) {
                $(".replenishmentContent").show();
                $(".noneDateMess").hide();

                for (let i = 0; i < replenishments.length; i++) {
                    var row = table.insertRow();
                    var cell1 = row.insertCell(0);
                    var cell2 = row.insertCell(1);
                    var cell3 = row.insertCell(2);
                    var cell4 = row.insertCell(3);
                    var cell5 = row.insertCell(4);

                    cell1.innerHTML = `${replenishments[i].partsNum}`;
                    cell1.className = 'parts_num';

                    cell2.innerHTML = `${replenishments[i].quantity}`;
                    cell2.className = 'quantity';

                    cell3.innerHTML = `${replenishments[i].restockStatus}`;
                    cell3.className = 'statusBtn';

                    if (replenishments[i].restockStatus == 1) {
                        cell3.innerHTML = `<button type="button" class="btn btn-warning btnRegister">開始</button>`;
                    } else if (replenishments[i].restockStatus == 2) {
                        cell3.innerHTML = `<button type="button" class="btn btn-success btnRegister btnEnd">完了</button>`;
                    }

                    cell4.innerHTML = `<button type="button" class="btn btn-secondary btnClose"><i class="fa-solid fa-xmark"></i></button>`;
                    cell5.innerHTML = `${replenishments[i].restockStatusId}`;
                    cell5.className = 'restockStatusId';
                }

            } else {
                $(".replenishmentContent").hide();
                $(".noneDateMess").show();
            }
        }



        var btnRegister = document.querySelectorAll('.btnRegister');　//　登録
        var buttonsClose = document.querySelectorAll('.btnClose');　//　欠品

        // ボタン押下時に確認ダイアログ表示
        btnRegister.forEach(function (button) {
            button.addEventListener('click', function () {
                var row = button.parentElement.parentElement;
                var statusBtn = button.textContent || button.innerText;
                var tdWithPartsNum = row.querySelector('.parts_num');
                var tdWithQuantity = row.querySelector('.quantity');
                var tdWithRestockStatusId = row.querySelector('.restockStatusId');

                // td要素のdata-timeとidを含むテキスト値を取得
                var dataRestockStatusId = tdWithRestockStatusId.textContent;
                var dataPartsNum = tdWithPartsNum.textContent;
                var dataQuantity = tdWithQuantity.textContent;

                // 開始ボタン押下時
                if (statusBtn == "開始") {
                    $.ajax({
                        type: 'POST',
                        url: baseUrl + '/Replenishment/UpdateForReplenishmentStart',
                        data: { dataRestockStatusId: dataRestockStatusId, statusBtn: statusBtn },
                        success: function (response) {
                            if (response.res != true) {
                                setTimeout(function () {
                                    Swal.fire({
                                        icon: 'error',
                                        title: `品番 ${dataPartsNum} 数量 ${dataQuantity}の<br>部品補充開始登録ができませんでした。<br>再度お試しください。`,
                                        html: `<span style="color: red;">${response.errorMessage}</span>`,
                                        confirmButtonColor: '#0d6efd',
                                        confirmButtonText: '閉じる',
                                        allowOutsideClick: false,
                                    })
                                }, 500);
                            }
                        }
                    }).done(function () {
                        setTimeout(function () {
                            $("#overlay").fadeOut(300);
                        }, 10);
                    });
                }

                // 終了ボタン押下時
                if (statusBtn == "完了") {
                    Swal.fire({
                        title: `品番 ${dataPartsNum} 数量 ${dataQuantity}の<br>部品補充完了登録を行います。<br>よろしいですか？`,
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#198754',
                        cancelButtonText: 'キャンセル',
                        allowOutsideClick: false,
                        confirmButtonText: '完了'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            $.ajax({
                                type: 'POST',
                                url: baseUrl + '/Replenishment/RegisterForReplenishmentComplete',
                                data: { dataRestockStatusId: dataRestockStatusId, statusBtn: statusBtn },
                                success: function (response) {
                                    if (response.res != true) {
                                        setTimeout(function () {
                                            Swal.fire({
                                                icon: 'error',
                                                title: `品番 ${dataPartsNum} 数量 ${dataQuantity}の<br>部品補充完了ができませんでした。<br>再度お試しください。`,
                                                html: `<span style="color: red;">${response.errorMessage}</span>`,
                                                confirmButtonColor: '#0d6efd',
                                                confirmButtonText: '閉じる',
                                                allowOutsideClick: false,
                                            })
                                        }, 500);
                                    }
                                }
                            }).done(function () {
                                setTimeout(function () {
                                    $("#overlay").fadeOut(300);
                                }, 500);
                            });
                        }
                    })
                }
            });
        });

        //　取消
        buttonsClose.forEach(function (button) {
            button.addEventListener('click', function () {
                var row = button.parentElement.parentElement;
                var statusBtn = button.textContent || button.innerText;
                var tdWithPartsNum = row.querySelector('.parts_num');
                var tdWithQuantity = row.querySelector('.quantity');
                var tdWithRestockStatusId = row.querySelector('.restockStatusId');

                // td要素のdata-timeとidを含むテキスト値を取得
                var dataRestockStatusId = tdWithRestockStatusId.textContent;
                var dataPartsNum = tdWithPartsNum.textContent;
                var dataQuantity = tdWithQuantity.textContent;

                $.ajax({
                    type: 'POST',
                    url: baseUrl + '/Replenishment/RegisterForReplenishmentCancel',
                    data: { dataRestockStatusId: dataRestockStatusId, statusBtn: statusBtn },
                    success: function (response) {
                        if (response.res != true) {
                            setTimeout(function () {
                                Swal.fire({
                                    icon: 'error',
                                    title: `箱種 ${dataPartsNum} 箱数 ${dataQuantity}の<br>部品補充の取消ができませんでした。<br>再度お試しください。`,
                                    html: `<span style="color: red;">${response.errorMessage}</span>`,
                                    confirmButtonColor: '#0d6efd',
                                    confirmButtonText: '閉じる',
                                    allowOutsideClick: false,
                                })
                            }, 500);
                        }
                    }
                }).done(function () {
                    setTimeout(function () {
                        $("#overlay").fadeOut(300);
                    }, 10);
                });
            });
        });
    }
}



// 自動的登録
function autoRegister(item) {
    setTimeout(() => {
        const dataSupplyId = item.partsSupplyRequestId;
        const dataEmptyBoxId = item.emptyBoxId;
        const dataIsPartsOnlyOder = item.isPartsOnlyOder;
        const machineNum = item.machineNum;
        const dataMachineNumber = item.machineNum;
        const dataPartsNum = item.partsNum;

        Swal.fire({
            title: `機番：${machineNum}<br>依頼をすべて完了で登録してもよろしいですか？`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#0d6efd',
            cancelButtonText: 'いいえ',
            confirmButtonText: 'はい',
            allowOutsideClick: false,
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    type: 'POST',
                    url: baseUrl + '/Parts/Complete',
                    data: { dataSupplyId: dataSupplyId, machineNum: dataMachineNumber, workType: workType, dataIsPartsOnlyOder: dataIsPartsOnlyOder, dataEmptyBoxId: dataEmptyBoxId },
                    success: function (response) {
                        if (response.res != true) {
                            setTimeout(function () {
                                Swal.fire({
                                    icon: 'error',
                                    title: `箱種 ${machineNum} 箱数 ${dataPartsNum}の<br>準備完了登録ができませんでした。<br>再度お試しください。`,
                                    html: `<span style="color: red;">${response.res}</span>`,
                                    confirmButtonColor: '#0d6efd',
                                    confirmButtonText: '閉じる',
                                    allowOutsideClick: false,
                                });
                            }, 500);
                        }
                    }
                }).done(function () {
                    setTimeout(function () {
                        $("#overlay").fadeOut(300);
                    }, 500);
                });
            } else {
                $.ajax({
                    type: 'POST',
                    url: baseUrl + '/Parts/Register',
                    data: { dataSupplyId: dataSupplyId, isRegister: false },
                    success: function (response) {
                        if (response.res != true) {
                            setTimeout(function () {
                                Swal.fire({
                                    icon: 'error',
                                    title: `機番 ${dataMachineNumber} 所番地 ${dataPartsNum}の<br>準備完了登録ができませんでした。<br>再度お試しください。`,
                                    html: `<span style="color: red;">${response.res}</span>`,
                                    confirmButtonColor: '#0d6efd',
                                    confirmButtonText: '閉じる',
                                    allowOutsideClick: false,
                                })
                            }, 500);
                        }
                    }
                }).done(function () {
                    setTimeout(function () {
                        $("#overlay").fadeOut(300);
                    }, 500);
                });
            }
        });
    }, 600); 
}


//　登録検出
function handleRegister(button, isRegister) {
    button.addEventListener('click', function () {
        var row = button.parentElement.parentElement;
        var dataSupplyId = row.querySelector('.supplyId').textContent;
        var dataMachineNumber = row.querySelector('.parts_num').textContent;
        var dataPartsNum = row.querySelector('.partsNum').textContent;

        $.ajax({
            type: 'POST',
            url: baseUrl + '/Parts/Register',
            data: { dataSupplyId: dataSupplyId, isRegister: isRegister },
            success: function (response) {
                if (response.res != true) {
                    setTimeout(function () {
                        Swal.fire({
                            icon: 'error',
                            title: `機番 ${dataMachineNumber} 所番地 ${dataPartsNum}の<br>準備完了登録ができませんでした。<br>再度お試しください。`,
                            html: `<span style="color: red;">${response.res}</span>`,
                            confirmButtonColor: '#0d6efd',
                            confirmButtonText: '閉じる',
                            allowOutsideClick: false,
                        })
                    }, 500);
                }
            }
        }).done(function () {
            setTimeout(function () {
                $("#overlay").fadeOut(300);
            }, 500);
        });
    });
}
// ----------------------------------------------------------------------//


// -----------------------------------在庫調整画面-----------------------------------//
// SignalRを使用して接続を初期化する
const isInventoryInformationPage = document.getElementById("inventory-information-page");
if (isInventoryInformationPage) {
    var connectionInventoryInformation = new signalR.HubConnectionBuilder().withUrl("inventoryAdjustmentHub").build();

    $(function () {
        connectionInventoryInformation.start().then(function () {
            InvokeinventoryInformations();
        })
    });

    // 短い遅延後に再接続を試みる
    var closeConnectTransportCount = 0;
    connectionInventoryInformation.onclose(function (error) {
        setTimeout(function () {
            connectionInventoryInformation.start().then(function () {
                InvokeinventoryInformations();
            })
            closeConnectTransportCount += 1;
            console.log("Error - onclose 再接続" + closeConnectTransportCount + "回目");
            // 接続が2回以上失われた場合はページをリロード
            if (closeConnectTransportCount >= 2)
                window.location.reload();
        }, 500);
    });

    // ページを離れた時やリロードしたタイミングで接続を止める
    window.addEventListener('unload', function () {
        console.log("addEventListener - unload");
        connectionInventoryInformation.stop();
    });

    // ハブのメソッドを呼び出す
    function InvokeinventoryInformations() {
        connectionInventoryInformation.invoke("SendInventoryInformations").catch(function (error) {
            // Controllerに接続できない場合はエラー
            console.log("Error - invoke catch");
            $(".connectionInventoryInformationError").text(error);
            $(".connectionInventoryInformationError").show();
        });
    }

    // エラー発生時
    connectionInventoryInformation.on("Error", (error) => {
        console.log("Error - on");
        $(".connectionInventoryInformationError").text(error);
        $(".connectionInventoryInformationError").show();
    });

    // グリッドに依頼をバインドする
    connectionInventoryInformation.on("ReceivedInventoryInformations", function (products) {
        BindInventoryInformationsToGrid(products);
    });
}

// グリッドに依頼をバインド
function BindInventoryInformationsToGrid(inventoryInformations) {
    $('#tblInventoryInformation tbody').empty();

    var tableDom = document.getElementById('tblInventoryInformation');

    if (tableDom !== null) {
        var table = tableDom.getElementsByTagName('tbody')[0];

        createTable(inventoryInformations, table);

        // テーブルを作成
        function createTable(inventoryInformations, table) {
            if (inventoryInformations.length > 0) {
                $(".inventoryInformationContent").show();
                $(".noneDateMess").hide();

                for (let i = 0; i < inventoryInformations.length; i++) {
                    var row = table.insertRow();
                    var cell1 = row.insertCell(0);
                    var cell2 = row.insertCell(1);
                    var cell3 = row.insertCell(2);
                    var cell4 = row.insertCell(3);
                    var cell5 = row.insertCell(4);

                    cell1.innerHTML = `${inventoryInformations[i].inventoryId}`;
                    cell1.className = 'inventoryId';

                    cell2.innerHTML = `${inventoryInformations[i].partsNum}`;
                    cell2.className = 'partsNum';

                    const inventoryNum = inventoryInformations[i].inventoryNum;

                    cell3.innerHTML = `${inventoryNum}`;
                    cell3.className = inventoryNum < 0 ? 'inventoryNum text-danger' : 'inventoryNum';

                    cell4.className = 'inventoryNumInput';
                    cell4.innerHTML = `<input type="number" name="inventoryNum" class="form-control inventoryNumInput" min="0" value="" onkeydown="return event.key !== '.' && event.key !== ','">`;

                    // statusBtn
                    cell5.className = 'statusBtn';
                    cell5.innerHTML = `<button type="button" class="btn btn-success btnRegister">登録</button>`;
                 
                }
            } else {
                $(".inventoryInformationContent").hide();
                $(".noneDateMess").show();
            }
        }

        var buttons = document.querySelectorAll('.btnRegister');

        // ボタン押下時に確認ダイアログ表示
        buttons.forEach(function (button) {
            button.addEventListener('click', function () {
                var row = button.parentElement.parentElement;
                var tdWithInventoryId = row.querySelector('.inventoryId');
                var tdWithPartsNum = row.querySelector('.partsNum');
                var tdWithInventoryNumInput = row.querySelector('input.inventoryNumInput');

                // td要素のdata-timeとidを含むテキスト値を取得
                var dataInventoryId = tdWithInventoryId.textContent;
                var dataPartsNum = tdWithPartsNum.textContent;
                var dataInventoryNumInput = tdWithInventoryNumInput.value;

                // 完了ボタン押下時
                $.ajax({
                    type: 'POST',
                    url: baseUrl + '/InventoryAdjustment/Register',
                    data: { dataInventoryId: dataInventoryId, dataInventoryNumInput: dataInventoryNumInput},
                    success: function (response) {
                        if (response.res != true) {
                            setTimeout(function () {
                                Swal.fire({
                                    icon: 'error',
                                    title: `ID ${dataInventoryId}　品番 ${dataPartsNum} 追加数 ${dataPartsNum}の<br>在庫調整ができませんでした。<br>再度お試しください。`,
                                    html: `<span style="color: red;">${response.res}</span>`,
                                    confirmButtonColor: '#0d6efd',
                                    confirmButtonText: '閉じる',
                                    allowOutsideClick: false,
                                })
                            }, 500);
                        }
                    }
                }).done(function () {
                    setTimeout(function () {
                        $("#overlay").fadeOut(300);
                    }, 10);
                });
            });
        });
    }
}

// マイナス数場合、テキストが赤くになる
$(document).on("input", ".inventoryNumInput", function () {
    const value = Number($(this).val());

    if (value < 0) {
        $(this).css("color", "red");
    } else {
        $(this).css("color", "");
    }
});

// ----------------------------------------------------------------------//


// 異なるテキストの長さに応じて文字サイズを調整
function countLengthText(cell) {
    var fullwidthCount = cell.innerText.length;
    if (fullwidthCount == 14)
        cell.style.fontSize = 23 + 'px';
    else if (fullwidthCount == 12)
        cell.style.fontSize = 25 + 'px';
    else if (fullwidthCount == 8)
        cell.style.fontSize = 27 + 'px';
    else if (fullwidthCount == 7)
        cell.style.fontSize = 28 + 'px';
    else if (fullwidthCount == 5)
        cell3.style.fontSize = 29 + 'px';
    else if (fullwidthCount <= 4)
        cell.style.fontSize = 30 + 'px';
}

// data-time を毎秒更新
function counttimer(element) {
    // サーバーから元のデータを取得
    if (element.dataset.timerStarted === "true") return;
    element.dataset.timerStarted = "true";

    const requestTime = new Date(element.getAttribute('data-request-datetime')).getTime();
    const countdownMinutes = parseInt(element.getAttribute("data-countdown"), 10) || 0;
    const countdownMs = countdownMinutes * 60000;

    const interval = setInterval(() => {
        const now = Date.now();
        // 経過時間を計算する（リクエストから現在まで）
        const elapsedMs = now - requestTime;
        //  残り時間（マイナスになる場合もあります）
        const remainingMs = countdownMs - elapsedMs;

        // data-time は経過した時間（経過時間）を保持
        const dataTime = elapsedMs;
        element.setAttribute("data-time", dataTime);
        if (elapsedMs >= 60 * 60000) {
            element.textContent = "59:59";
            element.className = 'timeCount redflag';
            clearInterval(interval);
            return;
        }

        let displayText;
        // ステータスを決定
        if (remainingMs > 0) {　// カウントダウン中（残り時間がまだある場合）
            const minutes = Math.floor(remainingMs / 60000);
            const seconds = Math.floor((remainingMs % 60000) / 1000);
            displayText = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
            element.className = 'timeCount';
        } else {　// カウントアップ中（カウントダウン終了後）
            const overMs = elapsedMs - countdownMs;
            const totalMinutes = countdownMinutes + Math.floor(overMs / 60000);
            const seconds = Math.floor((overMs % 60000) / 1000);
            const displayMinutes = Math.min(totalMinutes, 59);

            if (totalMinutes >= 59 && seconds >= 59) {　// 59:59 に到達した場合
                displayText = "59:59";
                element.className = 'timeCount redflag';
                clearInterval(interval);
            } else {　// 通常のカウントアップ表示
                displayText = `${displayMinutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
                element.className = 'timeCount redflag';
            }
        }

        element.textContent = displayText;
    }, 1000);
}

// ==================== 自動並び替え ==================== //


//　行を並べる
function sortTableRows() {
    // 供給ページか輸送ページかを判定
    const isSupplyPage = !!document.querySelector("#tblSupplyLeft");
    const isTransportPage = !!document.querySelector("#tblTransportLeft");
    if (!isSupplyPage && !isTransportPage) return;　// 対象ページでなければ処理しない

    // 左右のテーブルIDをページ種別に応じて設定
    const leftTableId = isSupplyPage ? "#tblSupplyLeft" : "#tblTransportLeft";
    const rightTableId = isSupplyPage ? "#tblSupplyRight" : "#tblTransportRight";
    // 左右テーブルの tbody を取得
    const leftTbody = document.querySelector(`${leftTableId} tbody`);
    const rightTbody = document.querySelector(`${rightTableId} tbody`);
    if (!leftTbody || !rightTbody) return;　// tbody が存在しなければ終了

    // 左右両方のテーブルからすべての行（tr）を取得
    const allRows = Array.from(document.querySelectorAll(`${leftTableId} tbody tr, ${rightTableId} tbody tr`));
    if (allRows.length === 0) return;

    // 並び替え用に各行データをパース
    const parsedRows = allRows.map(row => {
        const tdTime = row.querySelector(".timeCount");　// 時間表示セル
        const tdId = row.querySelector(isSupplyPage ? ".supplyId" : ".transportId");　// IDセル
        const timeText = tdTime?.textContent.trim() || "00:00";　// 表示時間
        const isCountUp = tdTime?.classList.contains("redflag"); // redflag = カウントアップ表示
        const id = parseInt(tdId?.textContent || "0", 10);　// ID 数値変換
        return { row, timeText, isCountUp, id };　// 並び替えに必要な情報を返す
    });


    // 並べる処理
    parsedRows.sort((a, b) => {
        // 「59:59」を最優先で先頭に並べる
        if (a.timeText === "59:59" && b.timeText !== "59:59") return -1;
        if (b.timeText === "59:59" && a.timeText !== "59:59") return 1;

        // カウントアップをカウントダウンより優先して前に並べる
        if (a.isCountUp && !b.isCountUp) return -1;
        if (!a.isCountUp && b.isCountUp) return 1;

        // 同じグループ内では時間を比較して並べる
        const timeA = a.timeText.split(':').map(Number);
        const timeB = b.timeText.split(':').map(Number);
        const totalA = timeA[0] * 60 + timeA[1];
        const totalB = timeB[0] * 60 + timeB[1];

        if (a.isCountUp) {
            // カウントアップ：時間の大きい順（降順）
            if (totalA > totalB) return -1;
            if (totalA < totalB) return 1;
        } else {
            // カウントダウン：時間の小さい順（昇順）
            if (totalA < totalB) return -1;
            if (totalA > totalB) return 1;
        }

        // 同値なら ID が大きい順
        return b.id - a.id;
    });

    leftTbody.innerHTML = "";
    rightTbody.innerHTML = "";
    parsedRows.forEach((item, index) => {
        if (index < 6) leftTbody.appendChild(item.row);
        else rightTbody.appendChild(item.row);
    });
}

setInterval(sortTableRows, 1000);

// ローディング表示
$(document).ajaxSend(function () {
    $("#overlay").fadeIn();
});

// 文字列の全角の長さを計算
function countFullwidthCharacters(str) {
    return Array.from(str).reduce(function (count, char) {
        return count + (char.match(/[^\x00-\x7F]/) ? 2 : 1);
    }, 0);
}

// 文字列の半角の長さを計算
function countHalfwidthCharacters(str) {
    return Array.from(str).reduce(function (count, char) {
        return count + (char.match(/[^\x00-\xFF]/) ? 1 : 0);
    }, 0);
}

// ページが完全にロードされるまでローディング表示
document.addEventListener("DOMContentLoaded", function () {
    $("#overlay").fadeIn();

    window.addEventListener("load", function () {
        $("#overlay").fadeOut();
    });
});


// ----------------------------------------------------------------------//
