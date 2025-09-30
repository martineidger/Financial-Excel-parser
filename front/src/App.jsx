import React, { useState, useEffect } from "react";
import axios from "axios";

const api = "https://localhost:7262/accounts";

function App() {
  const [files, setFiles] = useState([]);
  const [selectedFile, setSelectedFile] = useState(null);
  const [fileData, setFileData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [uploadStatus, setUploadStatus] = useState(null);
  const [expandedClasses, setExpandedClasses] = useState({});

  useEffect(() => {
    setLoading(true);
    axios.get(`${api}/files`)
      .then(res => setFiles(res.data.$values || []))
      .catch(err => console.error(err))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    if (selectedFile) {
      setLoading(true);
      axios.get(`${api}/files/${selectedFile}`)
        .then(res => setFileData(res.data))
        .catch(err => console.error(err))
        .finally(() => setLoading(false));
    }
  }, [selectedFile]);

  useEffect(() => {
    if (fileData?.accountBalances?.$values) {
      const initialState = {};
      fileData.accountBalances.$values.forEach(acc => {
        if (!(acc.accountClassName in initialState)) {
          initialState[acc.accountClassName] = false;
        }
      });
      setExpandedClasses(initialState);
    }
  }, [fileData]);

  const saveReportToJson = () => {
    if (!fileData) return;

    const dataStr = JSON.stringify(fileData, null, 2);
    const dataBlob = new Blob([dataStr], { type: "application/json" });

    const url = URL.createObjectURL(dataBlob);
    const link = document.createElement("a");
    link.href = url;
    link.download = `отчет_${fileData.bankName}_${new Date().toISOString().split('T')[0]}.json`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  };

  const uploadExcelFile = async (file, overwrite = false) => {
    try {
      setUploadStatus("uploading");
      const formData = new FormData();
      formData.append("file", file);

      const url = `${api}/upload${overwrite ? "?overwrite=true" : ""}`;

      await axios.post(url, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      setUploadStatus("success");

      const updatedFiles = await axios.get(`${api}/files`);
      setFiles(updatedFiles.data.$values || []);
    } catch (error) {
      console.error("Ошибка загрузки:", error);
      if (error.response?.status === 409 || error.message.includes("уже существует")) {
        const shouldOverwrite = window.confirm(`Файл "${file.name}" уже существует. Хотите перезаписать его?`);
        if (shouldOverwrite) {
          uploadExcelFile(file, true);
          return;
        }
      }
      setUploadStatus("error");
    }
  };

  const handleFileSelect = (event) => {
    const file = event.target.files[0];
    if (!file) return;

    if (!file.name.match(/\.(xls)$/)) {
      alert("Пожалуйста, выберите файл Excel с расширением .xlsx или .xls");
      return;
    }

    const existingFile = files.find(f => f.fileName === file.name);

    if (existingFile) {
      const shouldOverwrite = window.confirm(`Файл "${file.name}" уже существует. Хотите перезаписать его?`);
      if (shouldOverwrite) uploadExcelFile(file, true);
    } else {
      uploadExcelFile(file, false);
    }

    event.target.value = "";
  };

  const containerStyle = { fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif", padding: "0", backgroundColor: "#f8fafc", minHeight: "100vh", color: "#1e293b", width: "100vw", margin: "0" };
  const headerStyle = { textAlign: "center", marginBottom: "32px", padding: "32px 32px 24px 32px", borderBottom: "1px solid #e2e8f0", backgroundColor: "white", boxShadow: "0 2px 4px rgba(0, 0, 0, 0.05)" };
  const titleStyle = { fontSize: "28px", fontWeight: "700", color: "#0f172a", marginBottom: "8px" };
  const subtitleStyle = { fontSize: "16px", color: "#64748b", fontWeight: "400" };
  const contentStyle = { padding: "0 32px 32px 32px", maxWidth: "100%", margin: "0" };
  const cardStyle = { backgroundColor: "white", borderRadius: "12px", padding: "24px", boxShadow: "0 4px 6px -1px rgba(0,0,0,0.1), 0 2px 4px -1px rgba(0,0,0,0.06)", marginBottom: "24px", width: "100%", boxSizing: "border-box" };
  const selectContainerStyle = { display: "flex", alignItems: "center", gap: "12px", width: "100%" };
  const labelStyle = { fontSize: "14px", fontWeight: "600", color: "#374151", minWidth: "120px" };
  const selectStyle = { padding: "10px 16px", borderRadius: "8px", border: "1px solid #d1d5db", fontSize: "14px", fontWeight: "500", color: "#374151", backgroundColor: "white", flex: "1", outline: "none", transition: "all 0.2s ease", cursor: "pointer", maxWidth: "100%" };
  const selectHoverStyle = { borderColor: "#3b82f6", boxShadow: "0 0 0 3px rgba(59,130,246,0.1)" };
  const buttonStyle = { padding: "10px 20px", borderRadius: "8px", border: "none", backgroundColor: "#059669", color: "white", fontSize: "14px", fontWeight: "600", cursor: "pointer", transition: "all 0.2s ease", display: "flex", alignItems: "center", gap: "8px", minWidth: "180px", justifyContent: "center" };
  const buttonHoverStyle = { backgroundColor: "#047857", transform: "translateY(-1px)", boxShadow: "0 4px 8px rgba(5,150,105,0.3)" };
  const uploadButtonStyle = { ...buttonStyle, backgroundColor: "#3b82f6", minWidth: "160px" };
  const uploadButtonHoverStyle = { ...buttonHoverStyle, backgroundColor: "#2563eb" };
  const tableContainerStyle = { overflowX: "auto", borderRadius: "12px", boxShadow: "0 4px 6px -1px rgba(0,0,0,0.1), 0 2px 4px -1px rgba(0,0,0,0.06)", marginTop: "24px", width: "100%" };
  const tableStyle = { borderCollapse: "collapse", width: "100%", backgroundColor: "white", fontSize: "14px", minWidth: "1000px" };
  const thStyle = { padding: "16px 12px", backgroundColor: "#1e293b", color: "white", textAlign: "center", fontWeight: "600", fontSize: "13px", textTransform: "uppercase", letterSpacing: "0.5px", border: "none", whiteSpace: "nowrap" };
  const subThStyle = { ...thStyle, backgroundColor: "#334155", fontSize: "12px" };
  const tdStyle = { padding: "14px 12px", textAlign: "right", borderBottom: "1px solid #f1f5f9", color: "#475569", fontWeight: "500", whiteSpace: "nowrap" };
  const accountNumberStyle = { ...tdStyle, textAlign: "left", fontWeight: "600", color: "#1e293b" };
  const trHoverStyle = { backgroundColor: "#f8fafc", transition: "background-color 0.2s ease" };
  const loadingStyle = { display: "flex", justifyContent: "center", alignItems: "center", padding: "40px", color: "#64748b", fontSize: "14px", width: "100%" };
  const fileInfoStyle = { display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "20px", padding: "16px", backgroundColor: "#f8fafc", borderRadius: "8px", border: "1px solid #e2e8f0", width: "100%", boxSizing: "border-box" };
  const fileInfoLeftStyle = { flex: "1" };
  const fileInfoRightStyle = { display: "flex", alignItems: "center", gap: "16px" };
  const bankNameStyle = { fontSize: "18px", fontWeight: "700", color: "#0f172a", margin: "0 0 8px 0" };
  const periodStyle = { fontSize: "14px", color: "#64748b", fontWeight: "500", margin: "0" };
  const statsStyle = { textAlign: "right", marginRight: "20px" };
  const statsLabelStyle = { fontSize: "12px", color: "#64748b", marginBottom: "4px" };
  const statsValueStyle = { fontSize: "18px", fontWeight: "700", color: "#1e293b" };
  const uploadStatusStyle = { marginTop: "12px", padding: "8px 12px", borderRadius: "6px", fontSize: "14px", fontWeight: "500" };

  return (
    <div style={containerStyle}>
      <header style={headerStyle}>
        <h1 style={titleStyle}>Оборотная ведомость</h1>
        <p style={subtitleStyle}>Анализ финансовых операций и балансов счетов</p>
      </header>

      <div style={contentStyle}>
        <div style={cardStyle}>
          <div style={selectContainerStyle}>
            <label style={labelStyle}>Выберите файл:</label>
            <select
              style={selectStyle}
              onMouseOver={e => Object.assign(e.target.style, selectHoverStyle)}
              onMouseOut={e => Object.assign(e.target.style, selectStyle)}
              onChange={e => setSelectedFile(e.target.value)}
              value={selectedFile || ""}
            >
              <option value="">-- Выберите файл для анализа --</option>
              {files.map(file => (
                <option key={file.id} value={file.fileName}>{file.fileName}</option>
              ))}
            </select>
          </div>
        </div>

        <div style={cardStyle}>
          <div style={selectContainerStyle}>
            <label style={labelStyle}>Загрузить Excel файл:</label>
            <input
              type="file"
              id="excel-upload"
              accept=".xlsx,.xls"
              onChange={handleFileSelect}
              style={{ display: "none" }}
            />
            <button
              style={uploadButtonStyle}
              onMouseOver={e => Object.assign(e.target.style, uploadButtonHoverStyle)}
              onMouseOut={e => Object.assign(e.target.style, uploadButtonStyle)}
              onClick={() => document.getElementById("excel-upload").click()}
            >
              Загрузить Excel
            </button>
          </div>

          {uploadStatus === "uploading" && (
            <div style={{ ...uploadStatusStyle, color: "#3b82f6", backgroundColor: "#dbeafe" }}>⏳ Загрузка файла...</div>
          )}
          {uploadStatus === "success" && (
            <div style={{ ...uploadStatusStyle, color: "#059669", backgroundColor: "#dcfce7" }}>✅ Файл успешно загружен</div>
          )}
          {uploadStatus === "error" && (
            <div style={{ ...uploadStatusStyle, color: "#dc2626", backgroundColor: "#fee2e2" }}>❌ Ошибка загрузки файла</div>
          )}
        </div>

        {loading && <div style={loadingStyle}>Загрузка данных...</div>}

        {fileData && !loading && (
          <div style={cardStyle}>
            <div style={fileInfoStyle}>
              <div style={fileInfoLeftStyle}>
                <h2 style={bankNameStyle}>{fileData.bankName}</h2>
                <p style={periodStyle}>
                  Отчетный период: {new Date(fileData.periodStart).toLocaleDateString("ru-RU")} - {new Date(fileData.periodEnd).toLocaleDateString("ru-RU")}
                </p>
              </div>
              <div style={fileInfoRightStyle}>
                <div style={statsStyle}>
                  <div style={statsLabelStyle}>Всего счетов</div>
                  <div style={statsValueStyle}>{fileData.accountBalances.$values.length}</div>
                </div>
                <button
                  style={buttonStyle}
                  onMouseOver={e => Object.assign(e.target.style, buttonHoverStyle)}
                  onMouseOut={e => Object.assign(e.target.style, buttonStyle)}
                  onClick={saveReportToJson}
                >
                  Сохранить в JSON
                </button>
              </div>
            </div>

            <div style={tableContainerStyle}>
              <table style={tableStyle}>
                <thead>
                  <tr>
                    <th style={thStyle} rowSpan="2">Банковский счет</th>
                    <th style={thStyle} colSpan="2">Входящее сальдо</th>
                    <th style={thStyle} colSpan="2">Обороты</th>
                    <th style={thStyle} colSpan="2">Исходящее сальдо</th>
                  </tr>
                  <tr>
                    <th style={subThStyle}>Актив</th>
                    <th style={subThStyle}>Пассив</th>
                    <th style={subThStyle}>Дебет</th>
                    <th style={subThStyle}>Кредит</th>
                    <th style={subThStyle}>Актив</th>
                    <th style={subThStyle}>Пассив</th>
                  </tr>
                </thead>
                <tbody>
                  {fileData?.accountBalances?.$values?.length > 0 &&
                    (() => {
                      const accountsByClass = {};
                      fileData.accountBalances.$values.forEach(acc => {
                        if (!accountsByClass[acc.accountClassName]) {
                          accountsByClass[acc.accountClassName] = [];
                        }
                        accountsByClass[acc.accountClassName].push(acc);
                      });

                      const rows = [];

                      Object.entries(accountsByClass).forEach(([className, accounts]) => {
                        rows.push(
                          <tr
                            key={`class-${className}`}
                            style={{ backgroundColor: "#e2e8f0", cursor: "pointer" }}
                            onClick={() =>
                              setExpandedClasses(prev => ({
                                ...prev,
                                [className]: !prev[className],
                              }))
                            }
                          >
                            <td colSpan="7" style={{ textAlign: "left", fontWeight: "700", padding: "12px 14px" }}>
                              {expandedClasses[className] ? "▼" : "▶"} {className}
                            </td>
                          </tr>
                        );

                        if (expandedClasses[className]) {
                          accounts.forEach((acc, index) => {
                            rows.push(
                              <tr
                                key={`acc-${acc.$id || index}`}
                                style={{
                                  cursor: "default",
                                  backgroundColor: index % 2 === 0 ? "#fafbfc" : "white",
                                }}
                                onMouseEnter={e => Object.assign(e.currentTarget.style, trHoverStyle)}
                                onMouseLeave={e =>
                                  (e.currentTarget.style.backgroundColor = index % 2 === 0 ? "#fafbfc" : "white")
                                }
                              >
                                <td style={accountNumberStyle}>{acc.accountNumber}</td>
                                <td style={tdStyle}>{acc.inpBalanceActive.toLocaleString("ru-RU")}</td>
                                <td style={tdStyle}>{acc.inpBalancePassive.toLocaleString("ru-RU")}</td>
                                <td style={tdStyle}>{acc.turnoverDebit.toLocaleString("ru-RU")}</td>
                                <td style={tdStyle}>{acc.turniverCredit.toLocaleString("ru-RU")}</td>
                                <td style={tdStyle}>{acc.outpBalanceActive.toLocaleString("ru-RU")}</td>
                                <td style={tdStyle}>{acc.outpBalancePassive.toLocaleString("ru-RU")}</td>
                              </tr>
                            );
                          });
                        }
                      });

                      return rows;
                    })()}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default App;
